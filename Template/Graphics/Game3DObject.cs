using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace Template
{
    /// <summary>
    /// Base 3D object with position and rotation.
    /// </summary>
    class Game3DObject
    {
        public static float _PI = (float)Math.PI;
        public static float _2PI = (float)Math.PI * 2.0f;
        public static float _PI2 = (float)Math.PI / 2.0f;

        /// <summary>Position of object in virtual world.</summary>
        internal Vector4 _position;
        /// <summary>Position of object in virtual world.</summary>
        /// <value>Position of object in virtual world.</value>
        public Vector4 Position { get => _position; set { _position = value; } }

        /// <summary>Yaw angle - rotation around 0Y axis in DirectX (x - to left, y - to up, z - to back), rad.
        /// Order of applying: 1) Roll, 2) Pitch, 3) Yaw.
        /// Рыскание вокруг 0Y (x - продольная вперед, y - вертикальная вверх, z - поперечная вправо).</summary>
        internal float _yaw;
        /// <summary>Yaw angle - rotation around 0Y axis in DirectX (x - to left, y - to up, z - to back), rad.
        /// Order of applying: 1) Roll, 2) Pitch, 3) Yaw.
        /// Рыскание вокруг 0Y (x - продольная вперед, y - вертикальная вверх, z - поперечная вправо).</summary>
        /// <value>Yaw angle - rotation around 0Y axis in DirectX (x - to left, y - to up, z - to back), rad.</value>
        public float Yaw { get => _yaw; set => _yaw = value; }

        /// <summary>Pitch angle - rotation around 0X axis in DirectX (x - to left, y - to up, z - to back), rad.
        /// Order of applying: 1) Roll, 2) Pitch, 3) Yaw.
        /// Тангаж по нормальному вокруг 0Z (x - продольная вперед, y - вертикальная вверх, z - поперечная вправо).</summary>
        internal float _pitch;
        /// <summary>Pitch angle - rotation around 0X axis in DirectX (x - to left, y - to up, z - to back), rad.
        /// Order of applying: 1) Roll, 2) Pitch, 3) Yaw.
        /// Тангаж по нормальному вокруг 0Z (x - продольная вперед, y - вертикальная вверх, z - поперечная вправо).</summary>
        /// <value>Pitch angle - rotation around 0X axis in DirectX (x - to left, y - to up, z - to back), rad.</value>
        public float Pitch { get => _pitch; set => _pitch = value; }

        /// <summary>Roll angle - rotation around 0Z axis in DirectX (x - to left, y - to up, z - to back), rad.
        /// Order of applying: 1) Roll, 2) Pitch, 3) Yaw.
        /// Крен по нормальному вокруг 0X (x - продольная вперед, y - вертикальная вверх, z - поперечная вправо).</summary>
        internal float _roll;
        /// <summary>Roll angle - rotation around 0Z axis in DirectX (x - to left, y - to up, z - to back), rad.
        /// Order of applying: 1) Roll, 2) Pitch, 3) Yaw.
        /// Крен по нормальному вокруг 0X (x - продольная вперед, y - вертикальная вверх, z - поперечная вправо).</summary>
        /// <value>Roll angle - rotation around 0Z axis in DirectX (x - to left, y - to up, z - to back), rad.</value>
        public float Roll { get => _roll; set => _roll = value; }

        /// <summary>
        /// Consructor. Sets initial position and rotation. Order of applying rotation: 1) Roll (z), 2) Pitch (x), 3) Yaw (y).
        /// </summary>
        /// <param name="initialPosition">Initial position of object in world.</param>
        /// <param name="yaw">Initial angle of rotation around 0Y axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="pitch">Initial angle of rotation around 0X axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="roll">Initial rotation around 0Z axis (x - to left, y - to up, z - to back), rad.</param>
        public Game3DObject(Vector4 initialPosition, float yaw = 0.0f, float pitch = 0.0f, float roll = 0.0f)
        {
            _position = initialPosition;
            _yaw = yaw;
            _pitch = pitch;
            _roll = roll;
        }

        /// <summary>Rotate around 0Y axis by deltaYaw (x - to left, y - to up, z - to back), rad.</summary>
        /// <param name="deltaYaw">Angle, rad.</param>
        public virtual void YawBy(float deltaYaw)
        {
            _yaw += deltaYaw;
            if (_yaw > _PI) _yaw -= _2PI;
            else if (_yaw < -_PI) _yaw += _2PI;
        }

        /// <summary>Rotate around 0X axis by deltaPitch (x - to left, y - to up, z - to back), rad.</summary>
        /// <param name="deltaPitch">Angle, rad.</param>
        public virtual void PitchBy(float deltaPitch)
        {
            _pitch += deltaPitch;
            if (_pitch > _PI) _pitch -= _2PI;
            else if (_pitch < -_PI) _pitch += _2PI;
        }

        /// <summary>Rotate around 0Z axis by deltaRoll (x - to left, y - to up, z - to back), rad.</summary>
        /// <param name="deltaRoll">Angle, rad.</param>
        public virtual void RollBy(float deltaRoll)
        {
            _roll += deltaRoll;
            if (_roll > _PI) _roll -= _2PI;
            else if (_roll < -_PI) _roll += _2PI;
        }

        public virtual void MoveBy(float dX, float dY, float dZ)
        {
            _position.X += dX;
            _position.Y += dY;
            _position.Z += dZ;
        }

        public virtual void MoveTo(float x, float y, float z)
        {
            _position.X = x;
            _position.Y = y;
            _position.Z = z;
        }

        /// <summary>Get world transform matrix.</summary>
        /// <returns>World transform matrix.</returns>
        public Matrix GetWorldMatrix()
        {
            return Matrix.Multiply(Matrix.RotationYawPitchRoll(_yaw, _pitch, _roll), Matrix.Translation((Vector3)_position));
        }
    }
}
