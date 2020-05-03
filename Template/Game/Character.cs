using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    /// <summary>
    /// Character object.
    /// </summary>
    public class Character : Game3DObject
    {
        public BoundingBox BoxCollider;
        public Ray ray;
        /// <summary>Speed of character movements.</summary>
        private float _speed;
        /// <summary>Speed of character movements.</summary>
        /// <value>Speed of character movements.</value>
        public float Speed { get => _speed; set => _speed = value; }

        /// <summary>
        /// Constructor sets initial position, rotation angles and speed.
        /// </summary>
        /// <param name="initialPosition">Initial position.</param>
        /// <param name="yaw">Initial angle of rotation around 0Y axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="pitch">Initial angle of rotation around 0X axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="roll">Initial rotation around 0Z axis (x - to left, y - to up, z - to back), rad.</param>
        public Character(Vector4 initialPosition, float yaw = 0.0f, float pitch = 0.0f, float roll = 0.0f, float speed = 1.0f) :
            base(initialPosition, yaw, pitch, roll)
        {
            BoxCollider = new BoundingBox();
            ray = new Ray();
            _speed = speed;
        }
        public void Crouch(float y)
        {
            _position.Y = y;
        }
        /// <summary>Move forward or backward (depends of moveBy sign: positive - forward). Yaw = 0 - look to -Z.</summary>
        /// <param name="moveBy">Amount of movement.</param>
        public void MoveForwardBy(float moveBy)
        {
            _position.X -= moveBy * (float)Math.Sin(_yaw);
            _position.Z -= moveBy * (float)Math.Cos(_yaw);
        }

        /// <summary>Move to right or to left (depends of moveBy sign: positive - to right).</summary>
        /// <param name="moveBy">Amount of movement.</param>
        public void MoveRightBy(float moveBy)
        {
            _position.X -= moveBy * (float)Math.Cos(_yaw);
            _position.Z += moveBy * (float)Math.Sin(_yaw);
        }

        /// <summary>Rotate around 0X axis by deltaPitch (x - to left, y - to up, z - to back), rad. In virtual world, where X0Z is
        /// ground plane and 0Y axis to up pitch angle saturated by -Pi/2 and Pi/2. Получается все по другому: roll - крен,
        /// pitch - рыскание, yaw - тангаж.</summary>
        /// <param name="deltaPitch">Angle, rad.</param>
        public override void PitchBy(float deltaPitch)
        {
            _pitch += deltaPitch;
            if (_PI2 < _pitch) _pitch = _PI2;
            else if (-_PI2 > _pitch) _pitch = -_PI2;
        }

        /// <summary>Rotate around 0Z axis by deltaRoll (x - to left, y - to up, z - to back), rad.</summary>
        /// <param name="deltaRoll">Angle, rad.</param>
        public override void RollBy(float deltaRoll)
        {
        }

        public void BoundingsMove(Camera camera)
        {
            float x = Position.X;
            float z = Position.Z;
            float y = Position.Y;
            BoxCollider.Minimum = new Vector3(x - 1, 0, z - 1);
            BoxCollider.Maximum = new Vector3(x + 1, y, z + 1);

            float yaw = camera.Yaw;
            float pitch = camera.Pitch;
            //Радиус сферы
            float radius = 6f;

            x = (float)(camera.Position.Z - radius * Math.Cos(pitch) * Math.Cos(yaw));
            y = (float)(camera.Position.X - radius * Math.Sin(yaw) * Math.Cos(pitch));
            z = (float)(camera.Position.Y + radius * Math.Sin(pitch));

            ray.Position = new Vector3(y, z, x);
            ray.Direction = new Vector3(camera.Position.X, camera.Position.Y, camera.Position.Z);
        }
    }
}
