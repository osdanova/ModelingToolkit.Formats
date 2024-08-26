using ModelingToolkit.Core;
using SharpGLTF.Animations;
using SharpGLTF.Geometry;
using SharpGLTF.Geometry.VertexTypes;
using SharpGLTF.Materials;
using SharpGLTF.Scenes;
using SharpGLTF.Schema2;
using System.Numerics;

namespace ModelingToolkit.Formats
{
    using static ModelingToolkit.Core.MtScene;
    using CURVE_QUATERNION = CurveBuilder<Quaternion>;
    using CURVE_VECTOR3 = CurveBuilder<Vector3>;
    // Structure definition
    using MESH = MeshBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints8>;
    using VERTEX = VertexBuilder<VertexPositionNormal, VertexColor1Texture1, VertexJoints8>;

    public class GltfExporter
    {
        public static SceneBuilder CreateScene(MtScene mtScene)
        {
            SceneBuilder scene = new SceneBuilder();
            bool isSingleModel = mtScene.Models.Count == 1;

            for (int i = 0; i < mtScene.Models.Count; i++)
            {
                MtModel mtModel = mtScene.Models[i].Model;

                //------------------------------------------------------------------------
                //  TEXTURES
                //------------------------------------------------------------------------

                List<ImageBuilder> imgBuilders = new List<ImageBuilder>();
                for (int j = 0; j < mtModel.Materials.Count; j++)
                {
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        mtModel.Materials[j].DiffuseTextureBitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                        imgBuilders.Add(ImageBuilder.From(new SharpGLTF.Memory.MemoryImage(memStream.ToArray())));
                    }
                }

                //------------------------------------------------------------------------
                //  SKELETON
                //------------------------------------------------------------------------

                // Root node
                NodeBuilder rootNode = new NodeBuilder(mtModel.Name);
                rootNode.LocalTransform = Matrix4x4.Identity; // TODO

                // Joint nodes
                List<NodeBuilder> joints = new List<NodeBuilder>();
                for (int j = 0; j < mtModel.Joints.Count; j++)
                {
                    MtJoint mtJoint = mtModel.Joints[j];
                    NodeBuilder parentNode = mtJoint.ParentId != null ? joints[mtJoint.ParentId.Value] : rootNode;

                    NodeBuilder joint = parentNode.CreateNode(mtJoint.Name);
                    joint.LocalTransform = mtJoint.RelativeTransformationMatrix.Value;
                    joints.Add(joint);
                }
                scene.AddNode(rootNode);

                //------------------------------------------------------------------------
                //  MESHES
                //------------------------------------------------------------------------
                for (int j = 0; j < mtModel.Meshes.Count; j++)
                {
                    MtMesh mtMesh = mtModel.Meshes[j];

                    // Material
                    MaterialBuilder matBuilder = new MaterialBuilder(mtMesh.Name + "_mat")
                                    .WithBaseColor(imgBuilders[mtMesh.MaterialId.Value])
                                    .WithDoubleSide(false);

                    // Mesh structure definition
                    MESH meshBuilder = new MESH(mtMesh.Name);
                    var primBuilder = meshBuilder.UsePrimitive(matBuilder);

                    // Create mesh
                    foreach (MtFace face in mtMesh.Faces)
                    {
                        MtVertex vertex0 = mtMesh.Vertices[face.VertexIndices[0]];
                        MtVertex vertex1 = mtMesh.Vertices[face.VertexIndices[1]];
                        MtVertex vertex2 = mtMesh.Vertices[face.VertexIndices[2]];

                        primBuilder.AddTriangle(
                            VertexToGltf2(vertex0),
                            VertexToGltf2(vertex1),
                            VertexToGltf2(vertex2)
                        );
                    }

                    scene.AddSkinnedMesh(meshBuilder, Matrix4x4.Identity, joints.ToArray());
                }
            }

            return scene;
        }

        internal static VERTEX VertexToGltf2(MtVertex mtVertex)
        {
            Vector3 absolutePosition = mtVertex.AbsolutePosition.Value;
            Vector3 normal = mtVertex.Normals != null ? mtVertex.Normals.Value : Vector3.Zero;
            Vector4 color = mtVertex.ColorRGBA != null ? mtVertex.ColorRGBA.Value : Vector4.Zero;
            Vector2 uvCoords = mtVertex.TextureCoordinates != null ? new Vector2(mtVertex.TextureCoordinates.Value.X, 1 - mtVertex.TextureCoordinates.Value.Y) : Vector2.Zero;
            List<(int, float)> weights = new List<(int, float)>();
            foreach (var weight in mtVertex.Weights)
            {
                weights.Add((weight.JointIndex.Value, weight.Weight.Value));
            }

            VertexPositionNormal vpn = new VertexPositionNormal(absolutePosition, normal);
            VertexColor1Texture1 vct = new VertexColor1Texture1(color, uvCoords);
            VertexJoints8 vs = new VertexJoints8(weights.ToArray());

            return new VERTEX(vpn, vct, vs);
        }

        /*
         * NOTES:
         * animationTrack: Animation name
         * 
         * X: Right from viewer
         * Y: Towards camera
         * Z: Up
         * 
         * NodeBuilder.UseTranslation => Applies absolute translation
         */
        //public static void AddAnimation(SceneHelper scene, AnimationBinary animation, string animationName = "Anim")
        //{
        //    List<KeyWrapper> initialPoses = new List<KeyWrapper>();
        //
        //    // INITIAL POSE
        //    foreach (Motion.InitialPose initialPose in animation.MotionFile.InitialPoses)
        //    {
        //        initialPoses.Add(new KeyWrapper { Value = initialPose.Value, JointId = initialPose.BoneId, Channel = initialPose.ChannelValue });
        //
        //        NodeBuilder jointNode = scene.Joints[initialPose.BoneId];
        //
        //        if (initialPose.ChannelValue == Motion.Channel.TRANSLATION_X)
        //        {
        //            CURVE_VECTOR3 cur = jointNode.UseTranslation(animationName);
        //            cur.WithPoint(0, cur.GetPoint(0) + new Vector3(initialPose.Value, 0, 0));
        //        }
        //        if (initialPose.ChannelValue == Motion.Channel.TRANSLATION_Y)
        //        {
        //            CURVE_VECTOR3 cur = jointNode.UseTranslation(animationName);
        //            cur.WithPoint(0, cur.GetPoint(0) + new Vector3(0, initialPose.Value, 0));
        //        }
        //        if (initialPose.ChannelValue == Motion.Channel.TRANSLATION_Z)
        //        {
        //            CURVE_VECTOR3 cur = jointNode.UseTranslation(animationName);
        //            cur.WithPoint(0, cur.GetPoint(0) + new Vector3(0, 0, initialPose.Value));
        //        }
        //        if (initialPose.ChannelValue == Motion.Channel.ROTATION_X)
        //        {
        //            CURVE_QUATERNION cur = jointNode.UseRotation(animationName);
        //            cur.WithPoint(0, cur.GetPoint(0) + Quaternion.CreateFromAxisAngle(Vector3.UnitX, initialPose.Value));
        //        }
        //        if (initialPose.ChannelValue == Motion.Channel.ROTATION_Y)
        //        {
        //            CURVE_QUATERNION cur = jointNode.UseRotation(animationName);
        //            cur.WithPoint(0, cur.GetPoint(0) + Quaternion.CreateFromAxisAngle(Vector3.UnitY, initialPose.Value));
        //        }
        //        if (initialPose.ChannelValue == Motion.Channel.ROTATION_Z)
        //        {
        //            CURVE_QUATERNION cur = jointNode.UseRotation(animationName);
        //            cur.WithPoint(0, cur.GetPoint(0) + Quaternion.CreateFromAxisAngle(Vector3.UnitZ, initialPose.Value));
        //        }
        //
        //        //CURVE_VECTOR3 cur123 = jointNode.UseTranslation(animationName);
        //        //Vector3 check = cur123.GetPoint(0);
        //    }
        //
        //    var modelCheck = scene.Scene.ToGltf2();
        //}
    }
}
