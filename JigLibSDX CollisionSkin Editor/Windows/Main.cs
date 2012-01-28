using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JigLibSDX.Collision;
using MonitoredUndo;
using SlimDX;
using SlimDX.Direct3D9;
using DI = SlimDX.DirectInput;

using JLG = JigLibSDX.Geometry;
using JLM = JigLibSDX.Math;
using JLU = JigLibSDX.Utils;
using JLC = JigLibSDX.Collision;
using JLP = JigLibSDX.Physics;

namespace JigLibSDX_CSE
{
    public partial class Main : Form
    {
        #region Rendering
        //directx
        private Device _graphigsDevice;
        private Direct3D _direct3D;

        //cam
        private Camera _camera;

        //line
        private Line _line;

        //effects
        private Effect _colorMod;

        //sky sphere
        private Mesh _sky;
        private Texture _skyTexture;

        //meshes
        private Mesh _box;
        private Mesh _sphere;
        private Mesh _capsuleEnd;
        private Mesh _capsuleCylinder;

        private Mesh _mesh;

        // Grid
        private bool _drawGrid = true;
        #endregion

        private Project _project;

        #region My Changes / Additions

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = _project.CollisionPrimitiveInfos[_project.SelectedCollisionPrimitiveInfo];
            if(item != null)
            {
                UndoService.Current[item].Undo();
            }

            UpdateUndoRedoMenuLabels(item);
        }

        private void UpdateUndoRedoMenuLabels(CollisionPrimitiveInfo item)
        {
            UndoToolStripMenuItem.Enabled = UndoService.Current[item].CanUndo;
            RedoToolStripMenuItem.Enabled = UndoService.Current[item].CanRedo;
        }

        private void RedoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var item = _project.CollisionPrimitiveInfos[_project.SelectedCollisionPrimitiveInfo];
            if (item != null)
            {
                UndoService.Current[item].Redo();
            }
        }
        #endregion

        #region Input
        private BasicInput _input;
        private bool _mouseOverPictureBox;
        private float _mouseOverPictureBoxInitX;
        private float _mouseOverPictureBoxInitY;
        private float _mouseOverPictureBoxX;
        private float _mouseOverPictureBoxY;
        private CollisionSkinAction _skinAction = CollisionSkinAction.Move;
        #endregion

        #region Help Attributes
        private FormWindowState _oldFormWindowState = FormWindowState.Normal;
        private bool _initialized = false;
        private bool _inAfterSelect = false;
        private bool _lastLeftMouseButtonState = false;
        private bool _resizingForm = false;
        private JLC.MaterialProperties _selectedMatrialProperties;
        #endregion

        #region Init
        public Main()
        {
            InitializeComponent();

            _project = new Project();
            UpdateProjectTree(ref _project);
            UpdateCullModeCheck(ref _project);

            _camera = new Camera(new Vector3(0f, 0f, -17f), new Vector3(0f, 0f, 0f), Vector3.UnitY, pictureBox.Width, pictureBox.Height, (float)Math.PI / 2f, 0.001f, 500f);
            _input = new BasicInput(this.Handle);

            LoadContent();
        }
        #endregion

        #region Load Content
        private void LoadContent()
        {
            try
            {
                PresentParameters presentParams = new PresentParameters();
                presentParams.BackBufferHeight = pictureBox.Height;
                presentParams.BackBufferWidth = pictureBox.Width;
                presentParams.BackBufferFormat = Format.A8R8G8B8;
                presentParams.DeviceWindowHandle = pictureBox.Handle;

                _direct3D = new Direct3D();
                _graphigsDevice = new Device(_direct3D, 0, DeviceType.Hardware, pictureBox.Handle, CreateFlags.HardwareVertexProcessing, presentParams);

                _camera.Reset(pictureBox.Width, pictureBox.Height);
                
                _graphigsDevice.SetRenderState(RenderState.AlphaBlendEnable, true);
                _graphigsDevice.SetRenderState(RenderState.SourceBlend, Blend.SourceAlpha);
                _graphigsDevice.SetRenderState(RenderState.DestinationBlend, Blend.BothInverseSourceAlpha);
                _graphigsDevice.SetRenderState(RenderState.Lighting, true);
                _graphigsDevice.SetRenderState(RenderState.CullMode, Cull.None);
                _graphigsDevice.SetRenderState(RenderState.FillMode, FillMode.Solid);
                _graphigsDevice.SetRenderState(RenderState.NormalizeNormals, true);
                _graphigsDevice.SetRenderState(RenderState.PointSize, 4f);

                Vector3 direction = new Vector3(-1f, 0f, -1f);
                direction.Normalize();

                Light light = new Light();
                light.Type = LightType.Directional;
                light.Direction = direction;
                light.Diffuse = Color.White;
                light.Ambient = Color.White;

                _graphigsDevice.SetLight(0, light);
                _graphigsDevice.EnableLight(0, true);

                direction = new Vector3(1f, 0f, 1f);
                direction.Normalize();

                light = new Light();
                light.Type = LightType.Directional;
                light.Direction = direction;
                light.Diffuse = Color.White;
                light.Ambient = Color.White;

                _graphigsDevice.SetLight(1, light);
                _graphigsDevice.EnableLight(1, true);

                Material material = new Material();
                material.Diffuse = Color.White;
                material.Ambient = Color.Gray;

                _graphigsDevice.Material = material;

                _line = new Line(_graphigsDevice);

                _sky = Mesh.FromFile(_graphigsDevice, @"Content\Meshes\skysphere.x", MeshFlags.Managed);
                _sky.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);

                _skyTexture = Texture.FromFile(_graphigsDevice, @"Content\Textures\sky.png", Usage.None, Pool.Managed);

                _colorMod = Effect.FromFile(_graphigsDevice, @"Content\Effects\Color.fx", ShaderFlags.None);

                if (File.Exists(_project.MeshFilePath))
                {
                    _mesh = LoadMesh(ref _project, true);
                }

                _box = Mesh.FromFile(_graphigsDevice, @"Content\Meshes\box.x", MeshFlags.Managed);
                _box.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);

                _sphere = Mesh.FromFile(_graphigsDevice, @"Content\Meshes\sphere.x", MeshFlags.Managed);
                _sphere.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);

                _capsuleEnd = Mesh.FromFile(_graphigsDevice, @"Content\Meshes\capsule_end.x", MeshFlags.Managed);
                _capsuleEnd.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);

                _capsuleCylinder = Mesh.FromFile(_graphigsDevice, @"Content\Meshes\capsule_cylinder.x", MeshFlags.Managed);
                _capsuleCylinder.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);

                _initialized = true;
            }
            catch (Exception e)
            {
                _initialized = false;
                MessageBox.Show(this, e.Message);
            }
        }
        #endregion

        #region Unload Content
        private void UnloadContent()
        {
            _mesh = null;
            _box = null;
            _sphere = null;
            _capsuleEnd = null;
            _capsuleCylinder = null;
            _line = null;
            _graphigsDevice = null;
            _direct3D = null;
        }
        #endregion

        #region Reset Content
        private void ResetContent()
        {
            UnloadContent();
            LoadContent();

            if (_initialized)
            {
                Draw();
            }
        }
        #endregion

        #region Update
        // Can't use the name "Update" because Forms already have Update().
        private void UpdateLogic()
        {
            bool drawFrame = false;

            float mouseDeltaX = 0f;
            float mouseDeltaY = 0f;
            float mouseDeltaZ = 0f;

            float distanceToTarget = 0f;

            bool controlIsPressed = false;

            if (_initialized)
            {
                _input.Update();
                distanceToTarget = _camera.DistanceToCamera.Length();

                if (_input.KeyboardAvailable)
                {
                    if (_input.KeyboardState.IsPressed(DI.Key.LeftControl) || _input.KeyboardState.IsPressed(DI.Key.RightControl))
                    {
                        controlIsPressed = true;
                    }
                }

                SetActionInformation(controlIsPressed);

                if (_input.MouseAvailable && _mouseOverPictureBox)
                {
                    foreach (DI.BufferedData<DI.MouseState> state in _input.MouseBufferedData)
                    {
                        mouseDeltaX += state.Data.X;
                        mouseDeltaY += state.Data.Y;
                        mouseDeltaZ += state.Data.Z;
                    }

                    if (controlIsPressed)
                    {
                        #region Skin Actions
                        if (projectTree.SelectedNode != null && projectTree.SelectedNode.Tag != null && (mouseDeltaX != 0f || mouseDeltaY != 0f) && _input.MouseState.IsPressed(0))
                        {
                            CollisionPrimitiveInfo info = (CollisionPrimitiveInfo)projectTree.SelectedNode.Tag;

                            Vector2 objectPosition = Vector2.Zero;
                            Vector2 objectRelativeMousePosition = Vector2.Zero;
                            Vector2 lastObjectRelativeMousePosition = Vector2.Zero;
                            
                            Vector3 lastPosition = Vector3.Zero;
                            Vector3 currentPosition = Vector3.Zero;

                            switch (_skinAction)
                            {
                                case CollisionSkinAction.Move:
                                case CollisionSkinAction.Scale:
                                case CollisionSkinAction.ScaleAll:
                                    lastPosition = _camera.ScreenToView(_mouseOverPictureBoxX - mouseDeltaX, _mouseOverPictureBoxY - mouseDeltaY);
                                    currentPosition = _camera.ScreenToView(_mouseOverPictureBoxX, _mouseOverPictureBoxY);
                                    break;

                                case CollisionSkinAction.Rotate:
                                    objectPosition = _camera.ViewToScreen(info.Position.X, info.Position.Y, info.Position.Z);
                                    objectRelativeMousePosition = new Vector2(_mouseOverPictureBoxX - objectPosition.X, _mouseOverPictureBoxY - objectPosition.Y);
                                    lastObjectRelativeMousePosition = new Vector2(objectRelativeMousePosition.X - mouseDeltaX, objectRelativeMousePosition.Y - mouseDeltaY);
                                    break;
                            }

                            switch (_skinAction)
                            {
                                case CollisionSkinAction.Move:
                                    info.Position += currentPosition - lastPosition;
                                    propertyGrid.SelectedObject = info;
                                    break;

                                case CollisionSkinAction.Rotate:
                                    float rotation = JLM.MathHelper.VectorToRadians(objectRelativeMousePosition)
                                                   - JLM.MathHelper.VectorToRadians(lastObjectRelativeMousePosition);

                                    info.Orientation = info.Orientation * Matrix.RotationAxis(_camera.GetFront(), -rotation);

                                    propertyGrid.SelectedObject = info;
                                    break;

                                // This is probably wrong. Math.Abs surely destroys the right scale a bit, but it kinda works and I won't invest any more time into this. Sorry.
                                case CollisionSkinAction.Scale:
                                    Vector3 scale = currentPosition - lastPosition;

                                    Vector3 lastPositionRelative = lastPosition - info.Position;
                                    Vector3 currentPositionRelative = currentPosition - info.Position;

                                    Console.WriteLine("{0}", scale);

                                    Matrix invertedOrientation = info.Orientation;
                                    invertedOrientation.Invert();

                                    scale = Vector3.TransformCoordinate(scale, invertedOrientation);

                                    scale.X = (float)Math.Abs(scale.X);
                                    scale.Y = (float)Math.Abs(scale.Y);
                                    scale.Z = (float)Math.Abs(scale.Z);

                                    if (lastPositionRelative.Length() > currentPositionRelative.Length())
                                    {
                                        Vector3.Negate(ref scale, out scale);
                                    }

                                    info.Scale += scale;
                                    propertyGrid.SelectedObject = info;
                                    break;

                                case CollisionSkinAction.ScaleAll:
                                    Vector3 scaleAll = currentPosition - lastPosition;
                                    scaleAll = new Vector3(scaleAll.Length());

                                    if (lastPosition.Length() > currentPosition.Length())
                                    {
                                        scaleAll = -scaleAll;
                                    }

                                    info.Scale += scaleAll;
                                    propertyGrid.SelectedObject = info;
                                    break;    
                            }

                            drawFrame = true;
                        }
                        #endregion
                    }
                    else
                    {
                        #region View Actions
                        if (_input.MouseState.IsPressed(0))
                        {
                            //if (mouseDeltaX == 0f && mouseDeltaY == 0f && _lastLeftMouseButtonState == false)
                            //{
                            //    TrySelectPrimitive(ref _project, _mouseOverPictureBoxX, _mouseOverPictureBoxY);
                            //}
                            //else
                            //{
                                _camera.RotateAroundTarget(mouseDeltaY * -0.01f, mouseDeltaX * -0.01f);
                                drawFrame = true;
                            //}
                        }

                        if (_input.MouseState.IsPressed(1))
                        {
                            _camera.Move(new Vector3(mouseDeltaX * distanceToTarget * -0.003f, mouseDeltaY * distanceToTarget * -0.003f, 0f));
                            drawFrame = true;
                        }

                        if (mouseDeltaZ != 0f)
                        {
                            _camera.Zoom(mouseDeltaZ * distanceToTarget * -0.001f);
                            drawFrame = true;
                        }
                        #endregion
                    }

                    _lastLeftMouseButtonState = _input.MouseState.IsPressed(0);
                }
            }

            if (drawFrame)
            {
                Draw();
            }
        }
        #endregion

        #region Update Timer
        private void updateTimer_Tick(object sender, EventArgs e)
        {
            UpdateLogic();
        }
        #endregion

        #region Update ProjectTree
        private void UpdateProjectTree(ref Project project)
        {
            TreeNode main;
            bool expanded = false;

            TreeNode[] nodes = new TreeNode[project.CollisionPrimitiveInfos.Count];
            Color4 color;

            CollisionPrimitiveInfo selectedInfo = null;
            TreeNode selectedNode = null;

            if (projectTree.Nodes.Count > 0)
            {
                expanded = projectTree.Nodes[0].IsExpanded;
            }

            projectTree.Nodes.Clear();

            if (project.CollisionPrimitiveInfos.Count > 0 && project.SelectedCollisionPrimitiveInfo != -1)
            {
                selectedInfo = project.CollisionPrimitiveInfos[project.SelectedCollisionPrimitiveInfo];
            }

            for (int i = 0; i < project.CollisionPrimitiveInfos.Count; i++)
            {
                color = project.CollisionPrimitiveInfos[i].Color;
                color.Alpha = 0f;

                switch (project.CollisionPrimitiveInfos[i].Primitive.Type)
                {
                    case (int)JLG.PrimitiveType.Box:
                        nodes[i] = new TreeNode("Box (" + color.ToArgb().ToString() + ")");
                        nodes[i].Tag = project.CollisionPrimitiveInfos[i];
                        break;

                    case (int)JLG.PrimitiveType.Sphere:
                        nodes[i] = new TreeNode("Sphere (" + color.ToArgb().ToString() + ")");
                        nodes[i].Tag = project.CollisionPrimitiveInfos[i];
                        break;

                    case (int)JLG.PrimitiveType.Capsule:
                        nodes[i] = new TreeNode("Capsule (" + color.ToArgb().ToString() + ")");
                        nodes[i].Tag = project.CollisionPrimitiveInfos[i];
                        break;
                }

                if ((CollisionPrimitiveInfo)nodes[i].Tag == selectedInfo)
                {
                    selectedNode = nodes[i];
                }
            }

            if (_mesh != null)
            {
                main = new TreeNode("Mesh-Based Project", nodes);
            }
            else
            {
                main = new TreeNode("Freehand Project", nodes);
            }

            if (expanded)
            {
                main.Expand();
            }

            projectTree.Nodes.Add(main);

            if (selectedNode != null)
            {
                projectTree.SelectedNode = selectedNode;
                propertyGrid.SelectedObject = selectedInfo;
                materialPropertyGrid.SelectedObject = selectedInfo.MaterialProperties;
            }

        }
        #endregion

        #region Draw
        private void Draw()
        {
            ExtendedMaterial[] extMat;
            
            _graphigsDevice.Clear(ClearFlags.Target | ClearFlags.ZBuffer, Color.Gray, 1.0f, 0);
            _graphigsDevice.BeginScene();

            _graphigsDevice.SetTransform(TransformState.View, _camera.View);
            _graphigsDevice.SetTransform(TransformState.Projection, _camera.Projection);
            _graphigsDevice.SetTransform(TransformState.World, Matrix.Identity);

            if (_drawGrid)
            {
                DrawGrids();
            }

            _graphigsDevice.SetRenderState(RenderState.FillMode, FillMode.Solid);
            _graphigsDevice.SetRenderState(RenderState.Lighting, false);

            #region Draw SkySphere
            extMat = _sky.GetMaterials();
            _graphigsDevice.Material = extMat[0].MaterialD3D;
            _graphigsDevice.SetTexture(0, _skyTexture);
            _sky.DrawSubset(0);
            #endregion

            _graphigsDevice.SetRenderState(RenderState.Lighting, true);
            _graphigsDevice.SetTexture(0, null);
            //_graphigsDevice.SetTransform(TransformState.World, _camera.FaceCamera());

            #region Draw Mesh
            if (_mesh != null)
            {
                _graphigsDevice.SetRenderState(RenderState.CullMode, _project.MeshCullMode);
                _graphigsDevice.SetRenderState(RenderState.FillMode, FillMode.Wireframe);

                int passes = _colorMod.Begin(FX.None);
                for (int p = 0; p < passes; p++)
                {
                    _colorMod.SetValue("ColorModifier", new Vector4(2f, 2f, 2f, 0.66f));

                    _colorMod.BeginPass(p);

                    extMat = _mesh.GetMaterials();
                    _graphigsDevice.Material = extMat[0].MaterialD3D;
                    _graphigsDevice.SetTexture(0, null);

                    for (int i = 0; i < _mesh.GetAttributeTable().Length; i++)
                    {
                        _mesh.DrawSubset(i);
                    }

                    //_graphigsDevice.SetRenderState(RenderState.CullMode, _project.MeshCullMode);
                    //_graphigsDevice.SetRenderState(RenderState.FillMode, FillMode.Solid);
                    //for (int i = 0; i < _mesh.GetAttributeTable().Length; i++)
                    //{
                    //    _mesh.DrawSubset(i);
                    //}
                    _colorMod.EndPass();
                }
                _colorMod.End();
            }
            #endregion

            #region Draw CollisionSkin Primitives

            JLG.Box box;
            JLG.Sphere sphere;
            JLG.Capsule capsule;

            if (_project.CollisionPrimitiveInfos != null)
            {
                _graphigsDevice.SetRenderState(RenderState.CullMode, Cull.None);

                int passes = _colorMod.Begin(FX.None);
                for (int p = 0; p < passes; p++)
                {
                    _graphigsDevice.SetRenderState(RenderState.FillMode, FillMode.Solid);
                    for (int i = 0; i < _project.CollisionPrimitiveInfos.Count; i++)
                    {
                        if (!_project.CollisionPrimitiveInfos[i].Hidden)
                        {
                            if (_project.SelectedCollisionPrimitiveInfo == i)
                            {
                                _colorMod.SetValue("ColorModifier", new Vector4(_project.CollisionPrimitiveInfos[i].Color.ToVector3(), 1f));
                            }
                            else
                            {
                                _colorMod.SetValue("ColorModifier", new Vector4(_project.CollisionPrimitiveInfos[i].Color.ToVector3(), 0.5f));
                            }

                            _colorMod.BeginPass(p);

                            switch (_project.CollisionPrimitiveInfos[i].Primitive.Type)
                            {
                                case (int)JLG.PrimitiveType.Box:
                                    extMat = _box.GetMaterials();
                                    _graphigsDevice.Material = extMat[0].MaterialD3D;

                                    box = (JLG.Box)_project.CollisionPrimitiveInfos[i].Primitive;
                                    _graphigsDevice.SetTransform(TransformState.World, Matrix.Scaling(box.SideLengths) * box.TransformMatrix);

                                    _box.DrawSubset(0);
                                    break;

                                case (int)JLG.PrimitiveType.Sphere:
                                    extMat = _sphere.GetMaterials();
                                    _graphigsDevice.Material = extMat[0].MaterialD3D;

                                    sphere = (JLG.Sphere)_project.CollisionPrimitiveInfos[i].Primitive;
                                    _graphigsDevice.SetTransform(TransformState.World, Matrix.Scaling(sphere.Radius, sphere.Radius, sphere.Radius) * sphere.TransformMatrix);

                                    _sphere.DrawSubset(0);
                                    break;

                                case (int)JLG.PrimitiveType.Capsule:
                                    extMat = _capsuleEnd.GetMaterials();
                                    _graphigsDevice.Material = extMat[0].MaterialD3D;

                                    capsule = (JLG.Capsule)_project.CollisionPrimitiveInfos[i].Primitive;
                                    DrawCapsule(capsule);
                                    break;
                            }

                            _colorMod.EndPass();
                        }
                    }
                }
                _colorMod.End();
            }
            #endregion

            _graphigsDevice.EndScene();
            _graphigsDevice.Present();
        }

        private void DrawGrids()
        {
            #region Variables
            bool drawGrid = true;
            float distanceToGrid = _camera.DistanceToCamera.Length();
            float lineLength = 80f;
            float thickLineStep = 10f;

            Vector3[] line = new Vector3[2];
            Color3 color = new Color3(1f, 1f, 1f);
            Color4 colorWithAlpha;

            float opacity = 0.33f;
            float lightOpacity = 0.15f;

            float opacityModXY = 0f;
            float opacityModXZ = 0f;
            float opacityModYZ = 0f;

            Matrix transformation = _camera.ViewProjection;

            Vector3[] lines;
            #endregion

            #region View Logic
            Vector3 lookAtWorldAxis = _camera.GetLookAtTargetAxis();

            if (lookAtWorldAxis.X != 0f && lookAtWorldAxis.Y != 0f)
            {
                opacityModXY = 3f;
            }

            if (lookAtWorldAxis.Y != 0f && lookAtWorldAxis.Z != 0f)
            {
                opacityModYZ = 3f;
            }

            if (lookAtWorldAxis.X != 0f && lookAtWorldAxis.Z != 0f)
            {
                opacityModXZ = 3f;
            }

            if (_camera.Orthogonal == false)
            {
                if (_camera.InViewFrustum(Vector3.Zero))
                {
                    if (distanceToGrid >= 50f)
                    {
                        lineLength = 50f;
                        thickLineStep = 10f;
                    }
                    else if (distanceToGrid < 50f && distanceToGrid >= 30f)
                    {
                        lineLength = 30f;
                        thickLineStep = 10f;
                    }
                    else if (distanceToGrid < 30f && distanceToGrid >= 15f)
                    {
                        lineLength = 10f;
                        thickLineStep = 10f;
                    }
                    else if (distanceToGrid < 15f && distanceToGrid >= 7f)
                    {
                        lineLength = 5f;
                        thickLineStep = 10f;
                    }
                    else if (distanceToGrid < 7f && distanceToGrid >= 1.5f)
                    {
                        lineLength = 1f;
                        thickLineStep = 10f;
                    }
                    else if (distanceToGrid < 5f)
                    {
                        drawGrid = false;
                    }
                }
                else
                {
                    drawGrid = false;
                }
            }
            #endregion

            if (drawGrid)
            {
                #region Draw XY
                if (opacityModXY > 0f)
                {
                    for (float i = -lineLength; i <= lineLength; i += 1f)
                    {
                        if (i % thickLineStep != 0f)
                        {
                            colorWithAlpha = new Color4(lightOpacity * opacityModXY, color.Red, color.Green, color.Blue);
                        }
                        else
                        {
                            colorWithAlpha = new Color4(opacity * opacityModXY, color.Red, color.Green, color.Blue);
                        }

                        if (i != 0)
                        {
                            line[0] = new Vector3(-lineLength, i, 0f);
                            line[1] = new Vector3(lineLength, i, 0f);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(i, -lineLength, 0f);
                            line[1] = new Vector3(i, lineLength, 0f);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);
                        }
                        else
                        {
                            line[0] = new Vector3(-lineLength, i, 0f);
                            line[1] = new Vector3(0f, i, 0f);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(0f, i, 0f);
                            line[1] = new Vector3(lineLength, i, 0f);
                            _line.DrawTransformed(line, transformation, new Color4(opacity * opacityModXY, 1f, 0f, 0f));

                            line[0] = new Vector3(i, -lineLength, 0f);
                            line[1] = new Vector3(i, 0, 0f);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(i, 0, 0f);
                            line[1] = new Vector3(i, lineLength, 0f);
                            _line.DrawTransformed(line, transformation, new Color4(opacity * opacityModXY, 0f, 1f, 0f));
                        }
                    }

                    //X
                    _line.Width = 2;

                    lines = new Vector3[2];

                    lines[0] = new Vector3(lineLength + 0.4f, 0.2f, 0f);
                    lines[1] = new Vector3(lineLength + 0.8f, -0.2f, 0f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXY, 1f, 0f, 0f));

                    lines[0] = new Vector3(lineLength + 0.8f, 0.2f, 0f);
                    lines[1] = new Vector3(lineLength + 0.4f, -0.2f, 0f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXY, 1f, 0f, 0f));

                    //Y
                    lines = new Vector3[2];

                    lines[0] = new Vector3(-0.2f, lineLength + 0.8f, 0f);
                    lines[1] = new Vector3(0f, lineLength + 0.6f, 0f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXY, 0f, 1f, 0f));

                    lines[0] = new Vector3(0.2f, lineLength + 0.8f, 0f);
                    lines[1] = new Vector3(0f, lineLength + 0.6f, 0f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXY, 0f, 1f, 0f));

                    lines[0] = new Vector3(0f, lineLength + 0.6f, 0f);
                    lines[1] = new Vector3(0f, lineLength + 0.4f, 0f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXY, 0f, 1f, 0f));

                    _line.Width = 1;
                }
                #endregion

                #region Draw XZ
                if (opacityModXZ > 0f)
                {
                    for (float i = -lineLength; i <= lineLength; i += 1f)
                    {
                        if (i % thickLineStep != 0f)
                        {
                            colorWithAlpha = new Color4(lightOpacity * opacityModXZ, color.Red, color.Green, color.Blue);
                        }
                        else
                        {
                            colorWithAlpha = new Color4(opacity * opacityModXZ, color.Red, color.Green, color.Blue);
                        }

                        if (i != 0)
                        {
                            line[0] = new Vector3(-lineLength, 0f, i);
                            line[1] = new Vector3(lineLength, 0f, i);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(i, 0f, -lineLength);
                            line[1] = new Vector3(i, 0f, lineLength);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);
                        }
                        else
                        {
                            line[0] = new Vector3(-lineLength, 0f, i);
                            line[1] = new Vector3(0f, 0f, i);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(0f, 0f, i);
                            line[1] = new Vector3(lineLength, 0f, i);
                            _line.DrawTransformed(line, transformation, new Color4(opacity * opacityModXZ, 1f, 0f, 0f));

                            line[0] = new Vector3(i, 0f, -lineLength);
                            line[1] = new Vector3(i, 0f, 0);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(i, 0f, 0);
                            line[1] = new Vector3(i, 0f, lineLength);
                            _line.DrawTransformed(line, transformation, new Color4(opacity * opacityModXZ, 0f, 0f, 1f));
                        }
                    }

                    //X
                    _line.Width = 2;

                    lines = new Vector3[2];

                    lines[0] = new Vector3(lineLength + 0.4f, 0f, 0.2f);
                    lines[1] = new Vector3(lineLength + 0.8f, 0f, -0.2f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXZ, 1f, 0f, 0f));

                    lines[0] = new Vector3(lineLength + 0.8f, 0f, 0.2f);
                    lines[1] = new Vector3(lineLength + 0.4f, 0f, -0.2f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXZ, 1f, 0f, 0f));

                    //Z
                    lines = new Vector3[4];

                    lines[0] = new Vector3(-0.2f, 0f, lineLength + 0.8f);
                    lines[1] = new Vector3(0.2f, 0f, lineLength + 0.8f);
                    lines[2] = new Vector3(-0.2f, 0f, lineLength + 0.4f);
                    lines[3] = new Vector3(0.2f, 0f, lineLength + 0.4f);
                    _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModXZ, 0f, 0f, 1f));

                    _line.Width = 1;
                }
                #endregion

                #region Draw YZ
                if (opacityModYZ > 0f)
                {
                    for (float i = -lineLength; i <= lineLength; i += 1f)
                    {
                        if (i % thickLineStep != 0f)
                        {
                            colorWithAlpha = new Color4(lightOpacity * opacityModYZ, color.Red, color.Green, color.Blue);
                        }
                        else
                        {
                            colorWithAlpha = new Color4(opacity * opacityModYZ, color.Red, color.Green, color.Blue);
                        }

                        if (i != 0)
                        {
                            line[0] = new Vector3(0f, i, -lineLength);
                            line[1] = new Vector3(0f, i, lineLength);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(0f, -lineLength, i);
                            line[1] = new Vector3(0f, lineLength, i);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);
                        }
                        else
                        {
                            line[0] = new Vector3(0f, i, -lineLength);
                            line[1] = new Vector3(0f, i, 0f);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(0f, i, 0f);
                            line[1] = new Vector3(0f, i, lineLength);
                            _line.DrawTransformed(line, transformation, new Color4(opacity * opacityModYZ, 0f, 0f, 1f));

                            line[0] = new Vector3(0f, -lineLength, i);
                            line[1] = new Vector3(0f, 0f, i);
                            _line.DrawTransformed(line, transformation, colorWithAlpha);

                            line[0] = new Vector3(0f, 0f, i);
                            line[1] = new Vector3(0f, lineLength, i);
                            _line.DrawTransformed(line, transformation, new Color4(opacity * opacityModYZ, 0f, 1f, 0f));
                        }

                        //Y
                        _line.Width = 2;

                        lines = new Vector3[2];

                        lines[0] = new Vector3(0f, lineLength + 0.8f, -0.2f);
                        lines[1] = new Vector3(0f, lineLength + 0.6f, 0f);
                        _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModYZ, 0f, 1f, 0f));

                        lines[0] = new Vector3(0f, lineLength + 0.8f, 0.2f);
                        lines[1] = new Vector3(0f, lineLength + 0.6f, 0f);
                        _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModYZ, 0f, 1f, 0f));

                        lines[0] = new Vector3(0f, lineLength + 0.6f, 0f);
                        lines[1] = new Vector3(0f, lineLength + 0.4f, 0f);
                        _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModYZ, 0f, 1f, 0f));

                        //Z
                        lines = new Vector3[4];

                        lines[0] = new Vector3(0f, 0.2f, lineLength + 0.8f);
                        lines[1] = new Vector3(0f, -0.2f, lineLength + 0.8f);
                        lines[2] = new Vector3(0f, 0.2f, lineLength + 0.4f);
                        lines[3] = new Vector3(0f, -0.2f, lineLength + 0.4f);
                        _line.DrawTransformed(lines, transformation, new Color4(opacity * opacityModYZ, 0f, 0f, 1f));

                        _line.Width = 1;
                    }
                }
                #endregion
            }
        }

        private void DrawCapsule(JLG.Capsule capsule)
        {
            Vector3 backEndPosition = Vector3.TransformCoordinate(new Vector3(0f, 0f, -capsule.Length / 2f), capsule.Orientation) + capsule.Position;
            Vector3 frontEndPosition = Vector3.TransformCoordinate(new Vector3(0f, 0f, capsule.Length / 2f), capsule.Orientation) + capsule.Position;

            _graphigsDevice.SetTransform(TransformState.World, Matrix.RotationY((float)Math.PI) 
                                                               * Matrix.Scaling(capsule.Radius, capsule.Radius, capsule.Radius) 
                                                               * capsule.Orientation 
                                                               * Matrix.Translation(frontEndPosition));
            _capsuleEnd.DrawSubset(0);

            _graphigsDevice.SetTransform(TransformState.World, Matrix.Scaling(capsule.Radius, capsule.Radius, capsule.Radius) 
                                                               * capsule.Orientation 
                                                               * Matrix.Translation(backEndPosition));
            _capsuleEnd.DrawSubset(0);

            _graphigsDevice.SetTransform(TransformState.World, Matrix.Scaling(capsule.Radius, capsule.Radius, capsule.Length) * capsule.TransformMatrix);
            _capsuleCylinder.DrawSubset(0);
        }
        #endregion

        #region Form Events
        private void Main_Resize(object sender, EventArgs e)
        {
            switch (this.WindowState)
            { 
                case FormWindowState.Maximized:
                    ResetContent();
                    break;

                case FormWindowState.Normal:
                    if (_oldFormWindowState != FormWindowState.Normal)
                    {
                        ResetContent();
                    }
                    else
                    {
                        _camera.Reset(pictureBox.Width, pictureBox.Height);
                        Draw();
                    }
                    break;
            }

            _oldFormWindowState = this.WindowState;
        }

        private void Main_ResizeEnd(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                ResetContent();
            }

            _resizingForm = false;
        }

        private void Main_ResizeBegin(object sender, EventArgs e)
        {
            _resizingForm = true;
        }

        private void Main_Paint(object sender, PaintEventArgs e)
        {
            if (_initialized)
            {
                Draw();
            }
        }

        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            _mouseOverPictureBox = true;
        }

        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            _mouseOverPictureBox = false;
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            _mouseOverPictureBoxX = e.X;
            _mouseOverPictureBoxY = e.Y;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            _mouseOverPictureBoxInitX = (float)e.X;
            _mouseOverPictureBoxInitY = (float)e.Y;
        }

        private void propertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {            
            Draw();
        }

        private void projectTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!_inAfterSelect)
            {
                _inAfterSelect = true;

                _project.SelectedCollisionPrimitiveInfo = _project.CollisionPrimitiveInfos.IndexOf((CollisionPrimitiveInfo)e.Node.Tag);
                UpdateProjectTree(ref _project);
                Draw();

                _inAfterSelect = false;
            }
        }

        private void renderToolsSplit_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (this.WindowState == _oldFormWindowState && !_resizingForm)
            {
                ResetContent();
                delayedDrawTimer.Start();
            }
        }

        // Prevent not full drawn frames. (I simply want to have a flicker free form and will try my best not to draw after every update.)
        private void delayedDrawTimer_Tick(object sender, EventArgs e)
        {
            Draw();
            delayedDrawTimer.Stop();
        }
        #endregion

        #region Menu Input
        #region File
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _project = new Project();
            _mesh = null;
            propertyGrid.SelectedObject = null;

            UpdateProjectTree(ref _project);
            Draw();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Project temp = OpenProject(out _mesh);

            if (temp != null)
            {
                _project = temp;

                if (_project.SelectedCollisionPrimitiveInfo != -1)
                {
                    propertyGrid.SelectedObject = _project.CollisionPrimitiveInfos[_project.SelectedCollisionPrimitiveInfo];
                }
                else
                {
                    propertyGrid.SelectedObject = null;
                }

                UpdateProjectTree(ref _project);
                Draw();
            }
        }
        
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProject(ref _project);
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveProjectAs(ref _project);
        }

        private void setMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Mesh temp = LoadMesh(ref _project, false);

            if (temp != null)
            {
                _mesh = temp;
                Draw();
                UpdateProjectTree(ref _project);
            }
        }

        private void deleteMeshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _mesh = null;
            _project.MeshFilePath = "";

            UpdateProjectTree(ref _project);
            Draw();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        #region Edit
        private void addBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddBox(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void addSphereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddSphere(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void addCapsuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCapsule(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void dupplicateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Duplicate(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Remove(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }
        #endregion

        #region View
        private void resetCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _camera.Position = new Vector3(0f, 0f, -17f);
            _camera.Target = Vector3.Zero;

            Draw();
        }

        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _camera.Position = _camera.Target + -Vector3.UnitZ * _camera.DistanceToCamera.Length();

            Draw();
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _camera.Position = _camera.Target + Vector3.UnitZ * _camera.DistanceToCamera.Length();

            Draw();
        }

        private void rightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _camera.Position = _camera.Target + -Vector3.UnitX * _camera.DistanceToCamera.Length();

            Draw();
        }

        private void leftToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _camera.Position = _camera.Target + Vector3.UnitX * _camera.DistanceToCamera.Length();

            Draw();
        }

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Vector3 camPosMod = new Vector3(0f, -1f, -0.001f);
            camPosMod.Normalize();

            _camera.Position = _camera.Target + camPosMod * _camera.DistanceToCamera.Length();

            Draw();
        }

        private void bottomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Vector3 camPosMod = new Vector3(0f, 1f, -0.001f);
            camPosMod.Normalize();

            _camera.Position = _camera.Target + camPosMod * _camera.DistanceToCamera.Length();

            Draw();
        }

        private void switchPerspectiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _camera.Orthogonal = !_camera.Orthogonal;
            Draw();
        }

        private void noneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _project.MeshCullMode = (int)Cull.None;
            UpdateCullModeCheck(ref _project);
            Draw();
        }

        private void counterclockwiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _project.MeshCullMode = (int)Cull.Counterclockwise;
            UpdateCullModeCheck(ref _project);
            Draw();
        }

        private void clockwiseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _project.MeshCullMode = (int)Cull.Clockwise;
            UpdateCullModeCheck(ref _project);
            Draw();
        }

        private void hideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_project.SelectedCollisionPrimitiveInfo != -1)
            {
                _project.CollisionPrimitiveInfos[_project.SelectedCollisionPrimitiveInfo].Hidden = !_project.CollisionPrimitiveInfos[_project.SelectedCollisionPrimitiveInfo].Hidden;
                Draw();
            }
        }

        private void showAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < _project.CollisionPrimitiveInfos.Count; i++)
            {
                _project.CollisionPrimitiveInfos[i].Hidden = false;
            }

            Draw();
        }

        private void drawGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _drawGrid = !_drawGrid;
            Draw();
        }
        #endregion

        #region Build
        private void primitivesOnlyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Code code = new Code(ref _project, false, true);
            code.ShowDialog(this);

            code = null;
        }

        private void functionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Code code = new Code(ref _project, false, false);
            code.ShowDialog(this);

            code = null;
        }

        private void physicsObjectFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SavePhysicsObjectFile(ref _project);
        }

        private void jiglibXPrimitivesOnlyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Code code = new Code(ref _project, true, true);
            code.ShowDialog(this);

            code = null;
        }

        private void jiglibXFunctionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Code code = new Code(ref _project, true, false);
            code.ShowDialog(this);

            code = null;
        }
        #endregion

        #region Help
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog(this);

            about = null;
        }
        #endregion
        #endregion

        #region Toolbar Input
        private void toolbar_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Name == "moveButton" || e.ClickedItem.Name == "rotateButton" || e.ClickedItem.Name == "scaleButton" || e.ClickedItem.Name == "scaleAllButton")
            {
                moveButton.Checked = false;
                rotateButton.Checked = false;
                scaleButton.Checked = false;
                scaleAllButton.Checked = false;

                switch (e.ClickedItem.Name)
                {
                    case "moveButton":
                        _skinAction = CollisionSkinAction.Move;
                        moveButton.Checked = true;
                        break;

                    case "rotateButton":
                        _skinAction = CollisionSkinAction.Rotate;
                        rotateButton.Checked = true;
                        break;

                    case "scaleButton":
                        _skinAction = CollisionSkinAction.Scale;
                        scaleButton.Checked = true;
                        break;

                    case "scaleAllButton":
                        _skinAction = CollisionSkinAction.ScaleAll;
                        scaleAllButton.Checked = true;
                        break;
                }
            }
        }

        private void addBoxButton_Click(object sender, EventArgs e)
        {
            AddBox(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void addSphereButton_Click(object sender, EventArgs e)
        {
            AddSphere(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void addCapsuleButton_Click(object sender, EventArgs e)
        {
            AddCapsule(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void deletePrimitiveButton_Click(object sender, EventArgs e)
        {
            Remove(ref _project);
            UpdateProjectTree(ref _project);
            Draw();
        }

        private void setMeshButton_Click(object sender, EventArgs e)
        {
            Mesh temp = LoadMesh(ref _project, false);

            if (temp != null)
            {
                _mesh = temp;
                Draw();
                UpdateProjectTree(ref _project);
            }
        }

        private void deleteMeshButton_Click(object sender, EventArgs e)
        {
            _mesh = null;
            _project.MeshFilePath = "";

            UpdateProjectTree(ref _project);
            Draw();
        }
        #endregion

        #region Project Operations
        private Project OpenProject(out Mesh mesh)
        {
            DialogResult dialogResult = DialogResult.None;
            mesh = null;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "JigLibSDX Project (*.jlcsp)|*.jlcsp";

                dialogResult = openFileDialog.ShowDialog(this);

                switch (dialogResult)
                {
                    case DialogResult.OK:
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(Project));
                        TextReader textReader = new StreamReader(openFileDialog.FileName);

                        Project project = (Project)xmlSerializer.Deserialize(textReader);
                        textReader.Close();

                        project.FilePath = openFileDialog.FileName;

                        mesh = LoadMesh(ref project, true);

                        return project;
                        //break;

                    default:
                        return null;
                        //break;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(this, "An error occured:\n" + e.Message, "Error", MessageBoxButtons.OK);
                return null;
            }
        }

        private bool SaveProject(ref Project project)
        {
            DialogResult dialogResult = DialogResult.None;
            bool save = false;
            string filePath = "";

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JigLibSDX Project (*.jlcsp)|*.jlcsp";

                if (File.Exists(project.FilePath))
                {
                    save = true;
                    filePath = project.FilePath;
                }
                else
                {
                    dialogResult = saveFileDialog.ShowDialog(this);

                    switch (dialogResult)
                    {
                        case DialogResult.OK:
                            save = true;
                            filePath = saveFileDialog.FileName;
                            break;

                        default:
                            save = false;
                            break;
                    }

                }

                if (save)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Project));
                    TextWriter textWriter = new StreamWriter(filePath);

                    xmlSerializer.Serialize(textWriter, project);
                    textWriter.Close();

                    project.FilePath = filePath;

                    return true;
                }
                else
                {
                    return false;
                }
                
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "An error occured:\n" + e.Message, "Error", MessageBoxButtons.OK);
                return false;
            }
        }

        private bool SaveProjectAs(ref Project project)
        {
            DialogResult dialogResult = DialogResult.None;
            bool save = false;

            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "JigLibSDX Project (*.jlcsp)|*.jlcsp";

                dialogResult = saveFileDialog.ShowDialog(this);

                switch (dialogResult)
                {
                    case DialogResult.OK:
                        save = true;
                        break;

                    default:
                        save = false;
                        break;
                }

                if (save)
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Project));
                    TextWriter textWriter = new StreamWriter(saveFileDialog.FileName);

                    xmlSerializer.Serialize(textWriter, project);
                    textWriter.Close();

                    project.FilePath = saveFileDialog.FileName;

                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(this, "An error occured:\n" + e.Message, "Error", MessageBoxButtons.OK);
                return false;
            }
        }
        #endregion

        #region Mesh Operations
        private Mesh LoadMesh(ref Project project, bool reload)
        {
            DialogResult dialogResult = DialogResult.None;
            bool load = false;

            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "DirectX9 Mesh (*.x)|*.x";

                if (!reload)
                {
                    dialogResult = openFileDialog.ShowDialog(this);

                    switch (dialogResult)
                    {
                        case DialogResult.OK:
                            load = true;
                            break;

                        default:
                            load = false;
                            break;
                    }
                }
                else
                {
                    load = true;
                }

                if (load)
                {
                    Mesh mesh = null;

                    if (reload)
                    {
                        if (project.MeshFilePath == null || project.MeshFilePath.Length == 0)
                        {
                            // ignore, probably a free hand project
                        }
                        else
                        {
                            if (File.Exists(project.MeshFilePath))
                            {
                                mesh = Mesh.FromFile(_graphigsDevice, project.MeshFilePath, MeshFlags.Managed);
                                mesh.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);
                            }
                            else
                            {
                                MessageBox.Show(this, "Mesh could not be loaded.", "Error", MessageBoxButtons.OK);
                            }
                        }
                    }
                    else
                    {
                        mesh = Mesh.FromFile(_graphigsDevice, openFileDialog.FileName, MeshFlags.Managed);
                        mesh.OptimizeInPlace(MeshOptimizeFlags.VertexCache | MeshOptimizeFlags.Compact | MeshOptimizeFlags.AttributeSort);

                        project.MeshFilePath = openFileDialog.FileName;
                    }

                    return mesh;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(this, "An error occured:\n" + e.Message, "Error", MessageBoxButtons.OK);
                return null;
            }
        }
        #endregion

        #region CollisionSkin Operations
        private void AddBox(ref Project project)
        {
            var info = new CollisionPrimitiveInfo(JLG.PrimitiveType.Box, new JLC.MaterialProperties(0.25f, 0.5f, 0.35f));
            project.CollisionPrimitiveInfos.Add(info);
            project.SelectedCollisionPrimitiveInfo = project.CollisionPrimitiveInfos.Count - 1;
        }

        private void AddSphere(ref Project project)
        {
            var info = new CollisionPrimitiveInfo(JLG.PrimitiveType.Sphere, new JLC.MaterialProperties(0.25f, 0.5f, 0.35f));
            project.CollisionPrimitiveInfos.Add(info);
            project.SelectedCollisionPrimitiveInfo = project.CollisionPrimitiveInfos.Count - 1;        
        }

        private void AddCapsule(ref Project project)
        {
            var info = new CollisionPrimitiveInfo(JLG.PrimitiveType.Capsule, new JLC.MaterialProperties(0.25f, 0.5f, 0.35f));
            project.CollisionPrimitiveInfos.Add(info);
            project.SelectedCollisionPrimitiveInfo = project.CollisionPrimitiveInfos.Count - 1;
        }

        private void Duplicate(ref Project project)
        {
            var currentPrimitive = project.CollisionPrimitiveInfos[project.SelectedCollisionPrimitiveInfo];
            if (project.SelectedCollisionPrimitiveInfo != -1)
            {
                var info = new CollisionPrimitiveInfo(currentPrimitive.PrimitiveType, currentPrimitive.MaterialProperties)
                               {
                                   MaterialProperties = currentPrimitive.MaterialProperties,
                                   Orientation = currentPrimitive.Orientation,
                                   Position = currentPrimitive.Position,
                                   Rotations = currentPrimitive.Rotations,
                                   Scale = currentPrimitive.Scale,
                               }; 

                //TODO: Change the position of the primitive oO? ... mhhh.
                project.CollisionPrimitiveInfos.Add(info);
                project.SelectedCollisionPrimitiveInfo = project.CollisionPrimitiveInfos.Count - 1;              
            }
            else
            {
                MessageBox.Show(this, "Select a primitive (box, sphere or capsule) first.", "No! You are ding it wrong :P ...", MessageBoxButtons.OK);
            }
        }

        private void Remove(ref Project project)
        {
            CollisionPrimitiveInfo info;

            if (project.SelectedCollisionPrimitiveInfo != -1)
            {
                info = project.CollisionPrimitiveInfos[project.SelectedCollisionPrimitiveInfo];
                project.CollisionPrimitiveInfos.Remove(info);
            }
            else
            {
                MessageBox.Show(this, "Select a primitive (box, sphere or capsule) first.", "No! You are ding it wrong :P ...", MessageBoxButtons.OK);
            }
        }
        #endregion

        #region Other Help-Functions
        private void UpdateCullModeCheck(ref Project project)
        {
            noneToolStripMenuItem.Checked = false;
            clockwiseToolStripMenuItem.Checked = false;
            counterclockwiseToolStripMenuItem.Checked = false;

            switch (_project.MeshCullMode)
            {
                case (int)Cull.None:
                    noneToolStripMenuItem.Checked = true;
                    break;

                case (int)Cull.Clockwise:
                    clockwiseToolStripMenuItem.Checked = true;
                    break;

                case (int)Cull.Counterclockwise:
                    counterclockwiseToolStripMenuItem.Checked = true;
                    break;

                default:
                    goto case (int)Cull.None;
                    break;
            }
        }

        private void SetActionInformation(bool controlPressed)
        {
            string newInformation = "";

            if (controlPressed)
            {
                switch (_skinAction)
                {
                    case CollisionSkinAction.Move:
                        newInformation = "Move the mouse to move the object relative to the view.";
                        break;

                    case CollisionSkinAction.Rotate:
                        newInformation = "Move the mouse around the object to rotate relative to the view.";
                        break;

                    case CollisionSkinAction.Scale:
                        newInformation = "Move the mouse to or away from the object to scale it relative to the view.";
                        break;

                    case CollisionSkinAction.ScaleAll:
                        newInformation = "Move the mouse to or away from the object to scale it in all dimensions.";
                        break;
                }

                actionInformation.ForeColor = Color.Blue;
            }
            else
            {
                actionInformation.ForeColor = DefaultForeColor;
                newInformation = "View mode active. Press 'Ctrl' to change to control mode.";
            }

            if (actionInformation.Text != newInformation)
            {
                actionInformation.Text = newInformation;
            }
        }
        #endregion

        private void materialPropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (_project != null && _project.SelectedCollisionPrimitiveInfo != -1)
            {
                _project.CollisionPrimitiveInfos[_project.SelectedCollisionPrimitiveInfo].MaterialProperties = (JLC.MaterialProperties)materialPropertyGrid.SelectedObject;
            }
        }

        private void primitivePropertiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrimitivePropertiesEditor editor = new PrimitivePropertiesEditor(_project.PrimitiveProperties);

            if (editor.ShowDialog(this) == DialogResult.OK)
            {
                _project.PrimitiveProperties = editor.PrimitiveProperties;
            }
        }

        private void SavePhysicsObjectFile(ref Project project)
        {
            if (project != null && project.CollisionPrimitiveInfos != null)
            {
                if (project.CollisionPrimitiveInfos.Count > 0)
                {
                    DialogResult dialogResult = DialogResult.None;
                    bool save = false;

                    try
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "JigLibSDX Physics Object (*.jlpo)|*.jlpo";

                        dialogResult = saveFileDialog.ShowDialog(this);

                        switch (dialogResult)
                        {
                            case DialogResult.OK:
                                save = true;
                                break;

                            default:
                                save = false;
                                break;
                        }

                        if (save)
                        {
                            JLC.CollisionSkin skin = new JLC.CollisionSkin();
                            
                            JLG.Box box;
                            JLG.Sphere sphere;
                            JLG.Capsule capsule;

                            JLG.Primitive primitive;

                            Quaternion orientation;
                            Vector3 scale;
                            Vector3 translation;

                            Vector3 position;

                            for (int i = 0; i < project.CollisionPrimitiveInfos.Count; i++)
                            {
                                switch ((JLG.PrimitiveType)project.CollisionPrimitiveInfos[i].Primitive.Type)
                                { 
                                    case JLG.PrimitiveType.Box:
                                        box = (JLG.Box)project.CollisionPrimitiveInfos[i].Primitive.Clone();

                                        box.Orientation.Decompose(out scale, out orientation, out translation);
                                        box.Position += Vector3.TransformCoordinate(box.SideLengths * -0.5f, Matrix.RotationQuaternion(orientation));

                                        skin.AddPrimitive(box, project.CollisionPrimitiveInfos[i].MaterialProperties);
                                        break;

                                    case JLG.PrimitiveType.Sphere:
                                        skin.AddPrimitive(project.CollisionPrimitiveInfos[i].Primitive.Clone(), project.CollisionPrimitiveInfos[i].MaterialProperties);
                                        break;

                                    case JLG.PrimitiveType.Capsule:
                                        capsule = (JLG.Capsule)project.CollisionPrimitiveInfos[i].Primitive.Clone();

                                        capsule.Orientation.Decompose(out scale, out orientation, out translation);
                                        capsule.Position += Vector3.TransformCoordinate(new Vector3(0f, 0f, capsule.Length * -0.5f), Matrix.RotationQuaternion(orientation));

                                        skin.AddPrimitive(capsule, project.CollisionPrimitiveInfos[i].MaterialProperties);
                                        break;
                                }
                            }
                            
                            JLU.PhysicsObjectFile.Save(saveFileDialog.FileName, true, skin, project.PrimitiveProperties, null);
                        }

                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(this, "An error occured:\n" + e.Message, "Error", MessageBoxButtons.OK);
                    }
                }
                else
                {
                    MessageBox.Show(this, "You need at least one primitive.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show(this, "Create a new project first.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void duplicateButton_Click(object sender, EventArgs e)
        {
            Duplicate(ref _project);
        }
    }
}
