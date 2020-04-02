using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public static class Animations
    {
        private static float angle = 0;
        private static bool swordAnimAge1 = true;
        private static bool swordAnimAge2 = false;
        private static bool swordAnimAge3 = false;
        private static bool zombieWalk = true;

        private static float x = 0;

        public static bool ImpactBySword(MeshObject sword)
        {
            if (angle < 0.8f && swordAnimAge1 && !swordAnimAge2)
            {
                sword.Pitch += angle;
                angle += 0.03f;
                return true;
            }
            else  if(angle > 0.8f && swordAnimAge1 && !swordAnimAge2)
            {
                swordAnimAge1 = false;
                swordAnimAge2 = true;
            } 
            if(angle > -1.0f && !swordAnimAge1 && swordAnimAge2)
            {
                sword.Pitch += angle;
                angle -= 0.08f;
                return true;
            }
            else
            {
                swordAnimAge2 = false;
                swordAnimAge3 = true;
            }
            if (angle < 0 && !swordAnimAge1 && !swordAnimAge2 && swordAnimAge3)
            {
                sword.Pitch += angle;
                angle += 0.12f;
                return true;
            }
            else if (angle > 0 && !swordAnimAge1 && !swordAnimAge2 && swordAnimAge3)
            {
                swordAnimAge1 = true;
                swordAnimAge2 = false;
                swordAnimAge3 = false;
                angle = 0;
                return false;
            }
            return true;
        }

        public static float SwordIdle()
        {
            if(x < 6.28f)
            {
                x += 0.05f;
            }
            else
            {
                x = 0;
            }
            return (float) Math.Sin(x);
        }

        public static void ZombieWalk(MeshObject legLeft, MeshObject legRight)
        {
            float angle = 0.8f;
            float speed = 0.05f;
            if (zombieWalk && legRight.Pitch < angle)
            {
                legLeft.Pitch -= speed;
                legRight.Pitch += speed;
            }
            else
            {
                zombieWalk = false;
            }
            
            if(!zombieWalk && legRight.Pitch > -angle)
            {
                legLeft.Pitch += speed;
                legRight.Pitch -= speed;
            }
            else
            {
                zombieWalk = true;
            }
        }
    }
}
