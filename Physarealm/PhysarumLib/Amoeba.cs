﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Grasshopper;
using Rhino;
using Rhino.Geometry;
using Physarealm.Environment;

namespace Physarealm 
{
    public class Amoeba : Particle, IDisposable
    {
        public Vector3d orientation { get; set; }
        private float tempfloatx;//a temporary accurate position x
        private float tempfloaty;//a temporary accurate position z
        private float tempfloatz;//a temporary accurate position z
        public int ID { get; private set; }
        public int curx { get; private set; }// u index
        public int cury { get; private set; }// v index
        public int curz { get; private set; }// w index
        private int tempx; // temporary u index
        private int tempy; // temporary v index
        private int tempz; // temporary v index
        public float _sensor_angle { get; set; }
        public float _rotate_angle { get; set; }
        public float _sensor_offset { get; set; }
        public int _detectDir { get; set; }
        public int _detectDirRSubd { get; set; }
        public int _detectDirPhySubd{get;set;}
        public int _deathDistance { get; set; }
        public float _max_speed { get; set; }
        private float _sensor_theta_step_angle { get; set; }
        private float _sensor_phy_step_angle { get; set; }
        public int _distance_traveled;
        private float _cur_speed;
        public bool _moved_successfully;
        public bool _divide {set; get; }
        public bool _die { set; get; }
        public float _depT { get; set; }
        private float _ca_torealease;
        public float _pcd { get; set; }
        //private int division_frequency_test = 5;
        //private int death_frequency_test = 5;
        public int _div_radius { get; set; }
        public int _die_radius { get; set; }
        public int _div_max { get; set; }
        public int _div_min { get; set; }
        public int _die_min { get; set; }
        public int _die_max { get; set; }
        //public int _unitized;
        //public float moveangle;
        public float[] sensor_data;
        //public Vector3d moved;2q
        public double _guide_factor { get; set; }
        public double _escape_p { get; set; }
        public Point3d prev_loc;
        public bool _both_dir_flag;
        public int _border_type;//0: border die, 1: border wrap, 2: border bounce

        public Amoeba() : base() { }
        public Amoeba(int id)
            : base()
        {
            ID = id;
            _ca_torealease = 3;
            _both_dir_flag = true;
        }
        public Amoeba(int id, float sensor_angle = (float) 45, float rotate_angle = 45, float sensor_offset = 7, int detectDir = 3, int deathDistance = 100, float max_speed = 3, float pcd = (float) 0.1, float dept = 3)
            : base()
        {
            ID = id;
            _sensor_angle = sensor_angle;
            _rotate_angle = rotate_angle;
            _sensor_offset = sensor_offset;
            _detectDir = detectDir;
            _deathDistance = deathDistance;
            _max_speed = max_speed;
            _cur_speed = _max_speed;
            _pcd = pcd;
            _depT = dept;
            if (detectDir < 3)
                detectDir = 3;
            _detectDirRSubd = 0;
            _detectDirPhySubd = detectDir - 3;
            _sensor_theta_step_angle = 360 / detectDir;
            _sensor_phy_step_angle = sensor_angle / _detectDirPhySubd;
            _ca_torealease = dept;
            _div_radius = 3;
            _die_radius = 2;
            _div_max = 10;
            _div_min = 0;
            _die_min = 0;
            _die_max = 123;
            _moved_successfully = true;
            _guide_factor = 0;
            _escape_p = 0;
            _both_dir_flag = true;
        }
        public void initializeAmoeba(AbstractEnvironmentType env, Libutility util)
        {
            do
            {
                tempfloatx = (float)util.getDoubleRand(env.getUMin(), env.getUMax());
                tempfloaty = (float)util.getDoubleRand(env.getVMin(), env.getVMax());
                tempfloatz = (float)util.getDoubleRand(env.getWMin(), env.getWMax());
                Point3d indexPos = env.getIndexByPosition(tempfloatx, tempfloaty, tempfloatz);
                tempx = (int)indexPos.X;
                tempy = (int)indexPos.Y;
                tempz = (int)indexPos.Z;
            }
            while (!iniSuccess(tempx, tempy, tempz, env, util));
            curx = tempx;
            cury = tempy;
            curz = tempz;
            //Location = new Point3d(curx, cury, curz);
            Location = env.getPositionByIndex(curx, cury, curz);
            occupyCell(curx, cury, curz, env);
            selectRandomDirection(util);
            prev_loc = Location;
        }
        public void initializeAmoeba(double x, double y, double z, AbstractEnvironmentType env, Libutility util)
        {
            Point3d indexLoca = env.getIndexByPosition(x, y, z);
            tempx = (int)indexLoca.X;
            tempy = (int)indexLoca.Y;
            tempz = (int)indexLoca.Z;
            curx = tempx;
            cury = tempy;
            curz = tempz;
            //Location = new Point3d(curx, cury, curz);
            Location = env.getPositionByIndex(curx, cury, curz);
            occupyCell(curx, cury, curz, env);
            selectRandomDirection(util);
            prev_loc = Location;
        }
        public void initializeAmoeba(double x, double y, double z,  int radius, AbstractEnvironmentType env, Libutility util)
        {
            Point3d IniIndexLoca = env.getIndexByPosition(x, y, z);
            int start_x = (int)IniIndexLoca.X - radius > 0 ? (int)IniIndexLoca.X - radius : 0;
            int start_y = (int)IniIndexLoca.Y - radius > 0 ? (int)IniIndexLoca.Y - radius : 0;
            int start_z = (int)IniIndexLoca.Z - radius > 0 ? (int)IniIndexLoca.Z - radius : 0;
            int end_x = (int)IniIndexLoca.X + radius < env.u ? (int)IniIndexLoca.X + radius : env.u - 1;
            int end_y = (int)IniIndexLoca.Y + radius < env.v ? (int)IniIndexLoca.Y + radius : env.v - 1;
            int end_z = (int)IniIndexLoca.Z + radius < env.w ? (int)IniIndexLoca.Z + radius : env.w - 1;
            do
            {
                tempx = util.getRand(start_x, end_x + 1);
                tempy = util.getRand(start_y, end_y + 1);
                tempz = util.getRand(start_z, end_z + 1);
            }
            while (!iniSuccess(tempx, tempy, tempz, env, util));
            curx = tempx;
            cury = tempy;
            curz = tempz;
            //Location = new Point3d(curx, cury, curz);
            Location = env.getPositionByIndex(curx, cury, curz);
            occupyCell(curx, cury, curz, env);
            selectRandomDirection(util);
            prev_loc = new Point3d(x, y, z);
        }
        public bool iniSuccess(int x, int y, int z, AbstractEnvironmentType env, Libutility util)
        {
            if (env.isOccupidByParticleByIndex(x, y, z) == true)
                return false;
            if (env.isWithinObstacleByIndex(x, y, z) && util.getRandDouble() > _escape_p)
                return false;
            return true;
        }
        public void occupyCell(int x, int y, int z, AbstractEnvironmentType env)
        {
            env.clearGridCellByIndex(curx, cury, curz);
            env.occupyGridCellByIndex(tempx, tempy, tempz, ID);
            curx = tempx;
            cury = tempy;
            curz = tempz;
            resetFloatingPointPosition(env);
            if (_moved_successfully)
                env.increaseTrailByIndex(curx, cury, curz, _ca_torealease);
        }
        public void doMotorBehaviors(AbstractEnvironmentType env, Libutility util)
        {
            _distance_traveled++;
            prev_loc = Location;
            //_cur_speed = _max_speed * (1 - _distance_traveled / _deathDistance);
            _cur_speed = _max_speed;
            if (env.getGriddataByIndex(curx, cury, curz) == 1)
                _distance_traveled = 0;
            _moved_successfully = false;
            if (util.getRandDouble() < _pcd)
            {
              selectRandomDirection(util);
              resetFloatingPointPosition(env);
              return;
            }
            Point3d curLoc = Location;
            curLoc.Transform(Transform.Translation(orientation));
            //Location = curLoc;
            tempfloatx = (float)curLoc.X;
            tempfloaty = (float)curLoc.Y;
            tempfloatz = (float)curLoc.Z;
            switch (_border_type) 
            {
                case 0:
                    if(env.constrainPos(ref tempfloatx, ref tempfloaty, ref tempfloatz,0))
                        selectRandomDirection(util);
                    break;
                case 1: 
                    env.constrainPos(ref tempfloatx, ref tempfloaty, ref tempfloatz, 1);
                    break;
                case 2:
                    env.constrainPos(ref tempfloatx, ref tempfloaty, ref tempfloatz, 0);
                    orientation = env.bounceOrientation(curLoc, orientation);
                    break;
                default:
                    break;
            }
            //if(env.constrainPos(ref tempfloatx, ref tempfloaty, ref tempfloatz))
            //    selectRandomDirection(util);
            Point3d temppos = env.getIndexByPosition(tempfloatx, tempfloaty, tempfloatz);
            tempx = (int)temppos.X;
            tempy = (int)temppos.Y;
            tempz = (int)temppos.Z;
            if (env.isOccupidByParticleByIndex(tempx, tempy, tempz))
            {
                selectRandomDirection(util);
                return;
            }
            else if (env.isWithinObstacleByIndex(tempx, tempy, tempz) && util.getRandDouble() > _escape_p) 
            {
                selectRandomDirection(util);
                return;
            }
            else
            {
                _moved_successfully = true;
                Location = new Point3d(tempfloatx, tempfloaty, tempfloatz);
                env.clearGridCellByIndex(curx, cury, curz);
                //env.agedata[curx, cury, curz]++;
                env.occupyGridCellByIndex(tempx, tempy, tempz, ID);
                curx = tempx;
                cury = tempy;
                curz = tempz;
                //float trailIncrement = calculateTrailIncrement(util);
                //env.increaseTrailByIndex(curx, cury, curz, trailIncrement);
                env.increaseTrailByIndex(curx, cury, curz, _ca_torealease);
                //if (_moved_successfully && !_die && _distance_traveled % division_frequency_test == 0)
                if (_moved_successfully && !_die)
                    doDivisionTest(env);
            }
        }
        public float calculateTrailIncrement(Libutility util)
        {
            return util.getIncrement(_distance_traveled, _deathDistance);
        }
        public void selectRandomDirection(Libutility util)
        {
            double randx = (util.getRandDouble() - 0.5) * 2;
            double randy = (util.getRandDouble() - 0.5) * 2;
            double randz = (util.getRandDouble() - 0.5) * 2;
            Vector3d randDir = new Vector3d(randx, randy, randz);
            Double leng = randDir.Length;
            Double factor = _cur_speed / leng;
            orientation = Vector3d.Multiply(factor, randDir);
            return;
        }
        public void selectRandomDirection(Libutility util, Vector3d preDir)
        {
            double randx = (util.getRandDouble() - 0.5) * 2;
            double randy = (util.getRandDouble() - 0.5) * 2;
            double randz = (util.getRandDouble() - 0.5) * 2;
            Vector3d randDir = new Vector3d(randx, randy, randz);
            randDir = Vector3d.Add(randDir, preDir);
            Double leng = randDir.Length;
            Double factor = _cur_speed / leng;
            orientation = Vector3d.Multiply(factor, randDir);
            return;
        }
        public void resetFloatingPointPosition(AbstractEnvironmentType env)
        {
            Location = env.getPositionByIndex(curx, cury, curz);
            //tempfloatx = curx;
            //tempfloaty = cury;
            //tempfloatz = curz;
            //Location = new Point3d(curx, cury, curz);
            return;
        }
        public void doSensorBehaviors(AbstractEnvironmentType env, Libutility util)
        {
            this.doDeathTest(env);
            orientation = env.projectOrientationToEnv(Location, orientation);
            int det_count = _detectDir * _detectDirPhySubd + 1;
            int max_item = 0;
            float max_item_phy = 0;
            float max_item_theta = 0;
            sensor_data = new float[det_count];
            //List<trailInfo> infos = new List<trailInfo>();
            //infos.Add(env.getOffsetTrailValue(curx, cury, curz, orientation, 0, 0, _sensor_offset, util));
            //float maxtrail = 0;
            //Point3d tgtPos = new Point3d();
            sensor_data[0] = env.getOffsetTrailValue(curx, cury, curz, orientation, 0, 0, _sensor_offset, util);
            int count_cur = 1;
            for (int i = 0; i < _detectDir; i++)
            {
                for (int j = 1; j <= _detectDirPhySubd; j++)
                {
                    sensor_data[count_cur] = env.getOffsetTrailValue(curx, cury, curz, orientation, j * _sensor_phy_step_angle, i * _sensor_theta_step_angle, _sensor_offset, util);
                    //infos.Add(env.getOffsetTrailValue(curx, cury, curz, orientation, j * _sensor_phy_step_angle, i * _sensor_theta_step_angle, _sensor_offset, util));
                    if (sensor_data[count_cur] > sensor_data[max_item])
                    {
                        max_item = count_cur;
                        max_item_phy = j * _sensor_phy_step_angle;
                        max_item_theta = i * _sensor_theta_step_angle;
                    }
                    count_cur++;
                }
            }
            /*foreach(trailInfo inf in infos)
            {
              if(inf.trailValue > maxtrail)
              {
                maxtrail = inf.trailValue;
                tgtPos = inf.targetPos;
              }
            }
            Vector3d newOri = Point3d.Subtract(tgtPos, new Point3d(curx, cury, curz));
            double curLength = newOri.Length;
            double scaleFactor = _cur_speed / curLength;
            orientation = Vector3d.Multiply(scaleFactor, newOri);
            //orientation = newOri;
            */

            rotate(max_item_phy * _rotate_angle / _sensor_angle, max_item_theta);
            guideOrientation();
        }
        public void rotate(float rotate_phy, float rotate_theta)
        {
            Vector3d orienOri = orientation;
            Vector3d toOri = orientation;
            float phyrad = rotate_phy * 3.1416F / 180;
            float thetarad = rotate_theta * 3.1416F / 180;
            Point3d intLoc = new Point3d(curx, cury, curz);
            Plane oriplane = new Plane(intLoc, orienOri);
            toOri.Transform(Transform.Rotation(phyrad, oriplane.YAxis, intLoc));
            toOri.Transform(Transform.Rotation(thetarad, oriplane.ZAxis, intLoc));
            //orientation.Rotate(phyrad, oriplane.YAxis);
            //orientation.Transform(thetarad, oriplane.ZAxis);

            double curLength = toOri.Length;
            double scaleFactor = _cur_speed / curLength;
            toOri = Vector3d.Multiply(scaleFactor, toOri);
            //moveangle = rotate_theta;
            //moved = toOri - orienOri;
            orientation = toOri;
        }
        public void doDivisionTest(AbstractEnvironmentType env)
        {
            _divide = false;
            if (env.isOutsideBorderRangeByIndex(curx, cury, curz))
                return;
            if (isWithinThresholdRange(curx, cury, curz, env))
            {
                _divide = true;
            }
        }
        public void doDeathTest(AbstractEnvironmentType env)
        {
            _die = false;
            if (env.isOutsideBorderRangeByIndex(curx, cury, curz) && _border_type != 2)
            {
                _die = true;
            }
            //if (env.envdata[curx, cury, curz] == 2)
            //  _die = true;
            //return;
            if (isOutsideSurvivalRange(curx, cury, curz, env))
                _die = true;
        }
        public bool isWithinThresholdRange(int x, int y, int z, AbstractEnvironmentType env)
        {
            int d = env.countNumberOfParticlesPresentByIndex(x, y, z, _div_radius);
            //_around = d;
            if (d >= _div_min && d <= _div_max)
                return true;
            else return false;
        }
        public bool isOutsideSurvivalRange(int x, int y, int z, AbstractEnvironmentType env)
        {
            if (_distance_traveled > _deathDistance)
            {
                return true;
            }
            double d = env.countNumberOfParticlesPresentByIndex(x, y, z, _die_radius);
            if (d < _die_min || d > _die_max)
                return true;
            else return false;
        }
        public void setBirthDeathCondition(int gw, int gmin, int gmax, int sw, int smin, int smax)
        {
            _div_radius = gw;
            _die_radius = sw;
            _div_max = gmax;
            _div_min = gmin;
            _die_min = smin;
            _die_max = smax;
        }
        private void guideOrientation()
        {
            Vector3d curOri = orientation;
            curOri.Unitize();
            if (_both_dir_flag)
            {
                if (curOri.Z > 0)
                    curOri.Z += _guide_factor;
                else
                    curOri.Z -= _guide_factor;
            }
            else 
            {
                curOri.Z = curOri.Z > 0 ? curOri.Z + _guide_factor : -curOri.Z + _guide_factor;
            }
            curOri = Vector3d.Multiply(_cur_speed, curOri);
            orientation = curOri;

        }

        public void Dispose()
        {
            sensor_data.Initialize();
        }
    }//end of Amoeba class
}
