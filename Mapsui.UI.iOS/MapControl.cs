using System;
using GLKBaseEffectDrawing;
using MonoTouch.GLKit;
using MonoTouch.OpenGLES;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace Mapsui.UI.iOS
{
    class MapControl : GLKViewController
    {
        float _rotation;
        uint _vertexArray;
        uint _vertexBuffer;
        EAGLContext _context;
        GLKBaseEffect _effect;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _context = new EAGLContext(EAGLRenderingAPI.OpenGLES2);

            if (_context == null)
                Console.WriteLine("Failed to create ES context");

            var view = (GLKView)View;
            view.Context = _context;
            view.DrawableDepthFormat = GLKViewDrawableDepthFormat.Format24;
            view.DrawInRect += Draw;

            SetupGl();
        }

        void SetupGl()
        {
            EAGLContext.SetCurrentContext(_context);

            _effect = new GLKBaseEffect { LightingType = GLKLightingType.PerPixel };

            _effect.Light0.Enabled = true;
            _effect.Light0.DiffuseColor = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
            _effect.Light0.Position = new Vector4(-5f, -5f, 10f, 1f);
            _effect.Light0.SpecularColor = new Vector4(1f, 0f, 0f, 1f);

            _effect.Light1.Enabled = true;
            _effect.Light1.DiffuseColor = new Vector4(1f, 0.4f, 0.4f, 1f);
            _effect.Light1.Position = new Vector4(15f, 15f, 10f, 1f);
            _effect.Light1.SpecularColor = new Vector4(1f, 0f, 0f, 1f);

            _effect.Material.DiffuseColor = new Vector4(0f, 0.5f, 1f, 1f);
            _effect.Material.AmbientColor = new Vector4(0f, 0.5f, 0f, 1f);
            _effect.Material.SpecularColor = new Vector4(1f, 0f, 0f, 1f);
            _effect.Material.Shininess = 20f;
            _effect.Material.EmissiveColor = new Vector4(0.2f, 0f, 0.2f, 1f);

            GL.Enable(EnableCap.DepthTest);

            GL.Oes.GenVertexArrays(1, out _vertexArray);
            GL.Oes.BindVertexArray(_vertexArray);

            GL.GenBuffers(1, out _vertexBuffer);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Monkey.MeshVertexData.Length * sizeof(float)),
                           Monkey.MeshVertexData, BufferUsage.StaticDraw);

            GL.EnableVertexAttribArray((int)GLKVertexAttrib.Position);
            GL.VertexAttribPointer((int)GLKVertexAttrib.Position, 3, VertexAttribPointerType.Float,
                                    false, 6 * sizeof(float), 0);

            GL.EnableVertexAttribArray((int)GLKVertexAttrib.Normal);
            GL.VertexAttribPointer((int)GLKVertexAttrib.Normal, 3, VertexAttribPointerType.Float,
                                    false, 6 * sizeof(float), 12);

            GL.Oes.BindVertexArray(0);
        }

        public override void Update()
        {
            float aspect = Math.Abs(View.Bounds.Size.Width / View.Bounds.Size.Height);

            Matrix4 projectionMatrix =
                Matrix4.CreatePerspectiveFieldOfView((float)(Math.PI * 65f / 180.0f),
                                                   aspect, 0.1f, 100.0f);

            _effect.Transform.ProjectionMatrix = projectionMatrix;

            Matrix4 modelViewMatrix = Matrix4.CreateTranslation(new Vector3(0f, 0f, -3.5f));
            modelViewMatrix = Matrix4.Mult(Matrix4.CreateFromAxisAngle(new Vector3(1f, 1f, 1f), _rotation), modelViewMatrix);

            _effect.Transform.ModelViewMatrix = modelViewMatrix;

            _rotation += (float)TimeSinceLastUpdate * 0.5f;
        }

        public void Draw(object sender, GLKViewDrawEventArgs args)
        {
            GL.ClearColor(0.65f, 0.65f, 0.65f, 1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Oes.BindVertexArray(_vertexArray);

            _effect.PrepareToDraw();

            GL.DrawArrays(BeginMode.Triangles, 0, Monkey.MeshVertexData.Length / 6);
        }

    }
}