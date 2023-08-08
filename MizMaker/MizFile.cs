using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using MizMaker.Lua;

namespace MizMaker
{
    public class MizFile
    {
        private const double MissionLength = 4 * 60 * 60;
        
        private static readonly Regex MizFileRx = new(Settings.Instance.MizRegex, RegexOptions.Compiled);
        private static readonly string[] CarrierBasedGroupTypes = {"plane", "helicopter", "static"};

        private readonly string _path;
        private readonly string _outFolder;
        private int _firstFreeGroupId;

        public MizFile(string path, string outFolder)
        {
            _path = path;
            _outFolder = outFolder;
        }

        public string FilePrefix => Path.GetFileName(_path).Split('_')[0];
        
        public string Theatre => Settings.Instance.Theatres[FilePrefix];

        public void ApplyProfile(Profile profile)
        {
            Console.WriteLine($" === Now on {profile.Name} === ");
            
            var miz = GetMission();
            GetFirstFreeGroupId(miz);

            miz["date"] = new LsonDict
            {
                ["Year"] = profile.StartTime.Year,
                ["Month"] = profile.StartTime.Month,
                ["Day"] = profile.StartTime.Day,
            };  
            miz["start_time"] = (int) profile.StartTime.TimeOfDay.TotalSeconds;

            miz["weather"] = new LsonDict
            {
                ["type_weather"] = 0,
                ["atmosphere_type"] = 0,
                ["groundTurbulence"] = profile.Turb * 0.304,
                ["enable_fog"] = profile.Fog.Enabled,
                ["enable_dust"] = false,
                ["dust_density"] = 0,
                ["qnh"] = profile.QNH / 0.029529980164712 * 0.0075006157584566,
                ["season"] = new LsonDict
                {
                    ["temperature"] = profile.Temp,
                },
                ["modifiedTime"] = true,
                ["name"] = "Custom",
                ["fog"] = profile.Fog.CreateLsonDict()
                ["wind"] = new LsonDict
                {
                    ["at8000"] = new LsonDict
                    {
                        ["speed"] = profile.Wind260.Meters,
                        ["dir"] = profile.Wind260.Reciprocal,
                    },
                    ["at2000"] = new LsonDict
                    {
                        ["speed"] = profile.Wind066.Meters,
                        ["dir"] = profile.DirKtsGnd.Reciprocal,
                    },
                    ["atGround"] = new LsonDict
                    {
                        ["speed"] = profile.DirKtsGnd.Meters,
                        ["dir"] = profile.DirKtsGnd.Reciprocal,
                    },
                },
                ["visibility"] = new LsonDict
                {
                    ["distance"] = 80000,
                },
                ["clouds"] = profile.Clouds.CreateLsonDict()
            };
            
            AdjustShips(miz, profile);

            var savePath = MakeCopy(profile.Name);
            Save(savePath, miz);
        }

        private void AdjustShips(LsonValue mission, Profile profile)
        {
            foreach (var coal in mission["coalition"].Values)
            {
                foreach (var cty in coal["country"].Values)
                {
                    if (cty.TryGetValue("ship", out var ship)
                        && ship.TryGetValue("group", out var groups))
                    {
                        foreach (var grp in groups.Values.ToList())
                        {
                            var newCourse = AdjustShipGroup(grp, profile, mission);
                            if (newCourse.HasValue)
                                ExplodeGroup(groups, grp, mission, newCourse.Value);
                        }
                    }
                }
            }
        }

        private Vec2d? AdjustShipGroup(LsonValue shipGroup, Profile profile, LsonValue mission)
        {
            Angle wantHdg = default;
            Vec2d newStart = default;
            double wantSpeed = 10d;

            foreach (var u in shipGroup["units"].Values)
            {
                var type = u["type"].GetString();
                if (type.StartsWith("CVN_"))
                {
                    if (profile.CvnSpawn == "")
                        return null;
                    
                    newStart = SpawnPointByName(mission, shipGroup, profile.CvnSpawn);
                    wantHdg = Angle.FromDeg(profile.CvnDir.Dir);
                    wantSpeed = profile.CvnDir.Meters;
                    break;
                }

                if (type.StartsWith("LHA_"))
                {
                    if (newStart == default)
                    { // only use LHA heading if group doesn't contain a CVN
                        if (profile.LhaSpawn == "")
                            return null;
                        
                        newStart = SpawnPointByName(mission, shipGroup, profile.LhaSpawn);
                        wantHdg = Angle.FromDeg(profile.LhaDir.Dir);
                        wantSpeed = profile.LhaDir.Meters;
                    }
                }
            }

            if (newStart == default)
                return null;
            
            Console.Write($"{shipGroup["name"]} needs to face {wantHdg.Deg:000}!");
            
            var wp1 = shipGroup["route"]["points"][1];
            var wp2 = shipGroup["route"]["points"][2];

            var oldStart = new Vec2d(wp1);
            var oldCourse = new Vec2d(wp2) - new Vec2d(wp1);

            var newCourse = new Vec2d(wantHdg, wantSpeed * MissionLength);
            var endPoint = newStart + newCourse;
            
            Console.WriteLine($" Actual: {newCourse.Angle.Deg:000}!");

            wp1["x"] = newStart.X;
            wp1["y"] = newStart.Y;
            wp2["eta"] = 0d;
            
            wp2["x"] = endPoint.X;
            wp2["y"] = endPoint.Y;
            wp2["speed"] = wantSpeed;
            wp2["eta"] = MissionLength;
            
            foreach (var unit in shipGroup["units"].Values) 
                AdjustUnitInFormation(unit, oldStart, oldCourse, newStart, newCourse);

            return newCourse;
        }

        private void ExplodeGroup(LsonValue groups, LsonValue templateGroup, LsonValue mission, Vec2d newCourse)
        {
            var masterUnit = templateGroup["units"].Values.First()["unitId"];
            
            foreach (var unit in templateGroup["units"].Values.Reverse())
            {
                LsonValue newGrp;
                var unitStart = new Vec2d(unit);

                if (masterUnit.Equals(unit["unitId"]))
                {
                    Console.Write($"'{unit["name"]}' is flagship of '{templateGroup["name"]}'.");
                    newGrp = templateGroup;
                }
                else{
                    Console.Write($"New group: '{unit["name"]}'.");
                    newGrp = CloneObject(templateGroup);
                    newGrp["groupId"] = _firstFreeGroupId++;
                    groups[groups.Keys.Max(x => x.GetIntLenient()) + 1] = newGrp;
                }

                newGrp["units"].Clear();
                newGrp["units"].Add(1, unit);
                newGrp["x"] = unitStart.X;
                newGrp["y"] = unitStart.Y;
                newGrp["name"] = unit["name"];

                var firstWp = newGrp["route"]["points"][1];
                firstWp["x"] = unitStart.X;
                firstWp["y"] = unitStart.Y;
                StripWpTasks(firstWp, unit);

                var lastWp = newGrp["route"]["points"][2];
                var lastWpPos = unitStart + newCourse;

                lastWp["x"] = lastWpPos.X;
                lastWp["y"] = lastWpPos.Y;
                
                Console.WriteLine($" HDG: {(lastWpPos - unitStart).Angle.Deg:000}");
                
                StripWpTasks(lastWp, unit);
                
                StripGrpTasks(newGrp, unit);

                var t = unit["type"].GetString();
                if (t.StartsWith("CVN_") || t.StartsWith("LHA_"))
                    AdjustCarrierBasedSpawns(unit, mission);
            }
        }

        private void StripGrpTasks(LsonValue group, LsonValue unit)
        {
            if (group.TryGetValue("tasks", out var tasks))
            {
                var oldTasks = tasks.Values.ToList();
                group["tasks"].Clear();
                var keyInt = 1;

                foreach (var task in tasks.Values)
                {
                    if (task.TryGetValue("params", out var taskParams)
                        && taskParams.TryGetValue("action", out var taskAction)
                        && taskAction.TryGetValue("params", out var actionParams)
                        && actionParams.TryGetValue("unitId", out var unitId))
                    {
                        if (unitId.GetIntLenient() == unit["unitId"].GetIntLenient())
                            group["tasks"].Add(keyInt++, task);
                    }
                    else
                        group["tasks"].Add(keyInt++, task);
                }
            }
        }

        private void StripWpTasks(LsonValue wp, LsonValue unit)
        {
            if (wp.TryGetValue("task", out var task)
                && task.TryGetValue("params", out var taskParams)
                && taskParams.TryGetValue("tasks", out var tasks))
            {
                var oldTasks = tasks.Values.ToList();
                taskParams["tasks"].Clear();
                var keyInt = 1;
                
                foreach (var taskObj in oldTasks)
                {
                    if (taskObj.TryGetValue("params", out var taskObjParams)
                        && taskObjParams.TryGetValue("action", out var taskObjAction)
                        && taskObjAction.TryGetValue("params", out var taskActionParams)
                        && taskActionParams.TryGetValue("unitId", out var unitId))
                    {
                        if (unitId.GetIntLenient() == unit["unitId"].GetIntLenient())
                            taskParams["tasks"].Add(keyInt++, taskObj);
                    }
                    else
                        taskParams["tasks"].Add(keyInt++, taskObj);
                }
            }
        }

        private LsonValue CloneObject(LsonValue inputDict)
        {
            var val = new LsonDict();
            foreach (var pair in inputDict.GetDictSafe())
            {
                if (pair.Value.IsContainer)
                    val[pair.Key] = CloneObject(pair.Value);
                else
                    val[pair.Key] = pair.Value;
            }
            return val;
        }
        
        private void AdjustUnitInFormation(LsonValue unit, Vec2d oldStart, Vec2d oldCourse, Vec2d newStart, Vec2d newCourse)
        {
            var oldAbsolutePos = new Vec2d(unit);
            var oldRelativePos = oldAbsolutePos - oldStart;

            var radial = oldRelativePos.Angle - oldCourse.Angle;
            Console.WriteLine($"  - {unit["name"]} is on radial {radial.Deg:000} @ {oldRelativePos.LengthNautical:F1} NM");

            var newRelativePos = new Vec2d(radial + newCourse.Angle, oldRelativePos.Length);
            var newAbsolutePos = newStart + newRelativePos;
            
            unit["__old_x"] = oldAbsolutePos.X;
            unit["__old_y"] = oldAbsolutePos.Y;
            unit["__old_hdg"] = oldCourse.Angle.Rad;
            
            unit["heading"] = newCourse.Angle.Rad;
            unit["x"] = newAbsolutePos.X;
            unit["y"] = newAbsolutePos.Y;
        }

        private void AdjustCarrierBasedSpawns(LsonValue carrier, LsonValue mission)
        {
            var carrierId = carrier["unitId"];
            
            foreach (var coal in mission["coalition"].Values)
            {
                foreach (var cty in coal["country"].Values)
                {
                    foreach (var groupType in CarrierBasedGroupTypes)
                    {
                        if (cty.TryGetValue(groupType, out var category)
                            && category.TryGetValue("group", out var groups))
                        {
                            foreach (var grp in groups.Values)
                            {
                                if (grp.TryGetValue("route", out var route)
                                    && route.TryGetValue("points", out var points)
                                    && points.Values.Any()
                                    && points.Values.First().TryGetValue("linkUnit", out var linkUnitId)
                                    && linkUnitId.Equals(carrierId))
                                {
                                    MoveGroupToCarrier(carrier, grp);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private void MoveGroupToCarrier(LsonValue carrier, LsonValue grp)
        {
            var newPos = new Vec2d(carrier);
            Angle? newHdg = null;
            
            if (carrier.Keys.Contains("__old_x"))
            {
                var oldCarrAbsPos = new Vec2d(carrier["__old_x"].GetDouble(), carrier["__old_y"].GetDouble());
                var oldUnitAbsPos = new Vec2d(grp);

                var oldUnitRelPos = oldUnitAbsPos - oldCarrAbsPos;
                var oldCourse = Angle.FromRad(carrier["__old_hdg"].GetDouble());

                var radial = oldUnitRelPos.Angle - oldCourse;

                var newCourse = Angle.FromRad(carrier["heading"].GetDouble());
                
                var newRelativePos = new Vec2d(radial + newCourse, oldUnitRelPos.Length);
                var newCarrAbsPos = new Vec2d(carrier);

                newPos = newCarrAbsPos + newRelativePos;
                
                if (grp.Keys.Contains("heading"))
                    newHdg = Angle.FromRad(grp["heading"].GetDouble()) + (newCourse - oldCourse);
            }

            grp["x"] = newPos.X;
            grp["y"] = newPos.Y;
            
            if (newHdg.HasValue)
                grp["heading"] = newHdg.Value.Rad;

            var firstPoint = grp["route"]["points"].Values.First();
            firstPoint["x"] = carrier["x"];
            firstPoint["y"] = carrier["y"];

            foreach (var unit in grp["units"].Values)
            {
                unit["x"] = newPos.X;
                unit["y"] = newPos.Y;
                
                if (newHdg.HasValue)
                    unit["heading"] = newHdg.Value.Rad;
            }
        }

        private Vec2d SpawnPointByName(LsonValue mission, LsonValue grp, string name)
        {
            name = name.Trim();
            
            var match = mission["triggers"]["zones"].Values.FirstOrDefault(x => x["name"].Equals(name));
            if (match != null)
            {
                Console.WriteLine($"{grp["name"]} has spawn point: {name}");
                return new Vec2d(match);
            }

            var startPoint = grp["route"]["points"][1];
            Console.WriteLine($"No spawn point for {grp["name"]}, using current position.");
            return new Vec2d(startPoint);
        }

        private LsonValue GetMission()
        {
            using var fs = File.OpenRead(_path);
            using var zip = new ZipArchive(fs);
            
            var file = zip.GetEntry("mission");
            using var fs2 = file.Open();
            using var sr = new StreamReader(fs2);

            return LsonVars.Parse(sr.ReadToEnd())["mission"];
        }

        private string MakeCopy(string profileName)
        {
            var m = MizFileRx.Match(Path.GetFileName(_path));

            var desiredName = m.Success 
                ? $"{m.Groups[1].Value}_{m.Groups[2].Value}_{profileName}.miz" 
                : Path.GetFileNameWithoutExtension(_path) + "_" + profileName + ".miz";

            var newPath = Path.Combine(_outFolder.Replace("__MAP__",Theatre), desiredName);
            
            File.Copy(_path, newPath, true);
            return newPath;
        }

        public static void Save(string savePath, LsonValue mission)
        {
            var contents = LsonVars.ToString(new Dictionary<string, LsonValue>
            {
                ["mission"] = mission
            });

            using var fs = File.Open(savePath, FileMode.Open, FileAccess.ReadWrite);
            using var zip = new ZipArchive(fs, ZipArchiveMode.Update);
            
            var file = zip.GetEntry("mission");
            file.Delete();

            file = zip.CreateEntry("mission");
            
            using var fs2 = file.Open();
            using var sr = new StreamWriter(fs2);
            
            sr.Write(contents);
        }

        private void GetFirstFreeGroupId(LsonValue mission)
        {
            var topValue = -1;
            
            foreach (var coal in mission["coalition"].Values)
            foreach (var cty in coal["country"].Values)
            foreach (var grpCtg in cty.Values.Where(x => x.IsContainer))
            foreach (var group in grpCtg.Values)
            {
                if (group.TryGetValue("groupId", out var groupId))
                {
                    var groupIdInt = groupId.GetIntLenient();
                    if (groupIdInt > topValue)
                        topValue = groupIdInt;
                }
            }

            _firstFreeGroupId = topValue + 1;
        }

        public void Finish(string templateOutFolder)
        {
            if (templateOutFolder == null)
                return;
            
            var fName = Path.GetFileName(_path);
            var destPath = Path.Combine(templateOutFolder.Replace("__MAP__", Theatre), fName);

            if (File.Exists(destPath))
                File.Delete(destPath);
            
            File.Move(_path!, destPath);
        }
    }
}