using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Template
{
    public interface IModel
    {
        bool CharacterCollision(Character c);
        void Render(Renderer renderer, Matrix viewMatrix, Matrix projectionMatrix);
        
    }
}
