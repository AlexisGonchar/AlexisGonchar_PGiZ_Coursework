using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer11 = SharpDX.Direct3D11.Buffer;
using Template.Graphics;

namespace Template
{
    /// <summary>
    /// 3D object with mesh.
    /// </summary>
    public class MeshObject : Game3DObject, IDisposable
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct VertexDataStruct
        {
            public Vector4 position;
            public Vector4 normal;
            public Vector4 color;
            public Vector2 texCoord0;
            public Vector2 texCoord1;
        }

        private string _name;
        public string Name { get => _name; }
        private int _index;
        public int Index { get => _index; set => _index = value; }

        private DirectX3DGraphics _directX3DGraphics;

        /// <summary>Renderer object.</summary>
        private Renderer _renderer;

        #region Vertices and Indexes
        /// <summary>Count of object vertices.</summary>
        public int _verticesCount;

        /// <summary>Array of vertex data.</summary>
        public VertexDataStruct[] _vertices;

        /// <summary>Vertex buffer DirectX object.</summary>
        private Buffer11 _vertexBufferObject;

        private VertexBufferBinding _vertexBufferBinding;

        /// <summary>Count of object vertex Indexes.</summary>
        private int _indexesCount;

        /// <summary>Array of object vertex indexes.</summary>
        private uint[] _indexes;

        private Buffer11 _indexBufferObject;
        #endregion

        public Material _material;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="renderer">Renderer object.</param>
        /// <param name="initialPosition">Initial position in 3d scene.</param>
        /// <param name="yaw">Initial angle of rotation around 0Y axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="pitch">Initial angle of rotation around 0X axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="roll">Initial rotation around 0Z axis (x - to left, y - to up, z - to back), rad.</param>
        /// <param name="vertices">Array of vertex data.</param>
        public MeshObject(string name, DirectX3DGraphics directX3DGraphics, Renderer renderer,
            Vector4 initialPosition, float yaw, float pitch, float roll,
            VertexDataStruct[] vertices, uint[] indexes, Material material) :
            base(initialPosition, yaw, pitch, roll)
        {
            _name = name;
            _directX3DGraphics = directX3DGraphics;
            _renderer = renderer;
            if (null != vertices)
            {
                _vertices = vertices;
                _verticesCount = _vertices.Length;
            }
            if (null != indexes)
            {
                _indexes = indexes;
                _indexesCount = _indexes.Length;
            } else
            {
                _indexesCount = _verticesCount;
                _indexes = new uint[_indexesCount];
                for (int i = 0; i <= _indexesCount; ++i) _indexes[i] = (uint)i;
            }
            _material = material;
            
            _vertexBufferObject = Buffer11.Create(_directX3DGraphics.Device, BindFlags.VertexBuffer, _vertices, Utilities.SizeOf<VertexDataStruct>() * _verticesCount);
            _vertexBufferBinding = new VertexBufferBinding(_vertexBufferObject, Utilities.SizeOf<VertexDataStruct>(), 0);
            _indexBufferObject = Buffer11.Create(_directX3DGraphics.Device, BindFlags.IndexBuffer, _indexes, Utilities.SizeOf<int>() * _indexesCount);
        }

        public virtual void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix)
        {
            Matrix worldMatrix = GetWorldMatrix();
            _renderer.UpdatePerObjectConstantBuffer(0, worldMatrix, viewMatrix, projectionMatrix);
            DeviceContext deviceContext = _directX3DGraphics.DeviceContext;
            _renderer.UpdateMaterialProperties(_material);
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.SetVertexBuffers(0, _vertexBufferBinding);
            deviceContext.InputAssembler.SetIndexBuffer(_indexBufferObject, Format.R32_UInt, 0);
            deviceContext.DrawIndexed(_indexesCount, 0, 0);
        }

        public void Dispose()
        {
            Utilities.Dispose(ref _indexBufferObject);
            Utilities.Dispose(ref _vertexBufferObject);
        }
    }
}
