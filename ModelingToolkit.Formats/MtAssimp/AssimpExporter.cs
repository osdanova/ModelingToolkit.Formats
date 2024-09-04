using ModelingToolkit.Core;

namespace ModelingToolkit.Formats.MtAssimp
{
    public class AssimpExporter
    {
        public static Assimp.Scene ExportModel(MtScene scene)
        {
            Assimp.Scene assimpScene = AssimpUtils.GetBaseScene();

            MtModel model = scene.Models[0].Model;

            // MATERIALS / TEXTURES
            for (int i = 0; i < model.Materials.Count; i++)
            {
                Assimp.TextureSlot texture = new Assimp.TextureSlot();
                texture.FilePath = Path.GetFileNameWithoutExtension(model.Materials[i].DiffuseTextureFileName);
                texture.TextureType = Assimp.TextureType.Diffuse;

                Assimp.Material mat = new Assimp.Material();
                mat.Name = model.Materials[i].Name;
                mat.TextureDiffuse = texture;
                assimpScene.Materials.Add(mat);
            }

            for (int i = 0; i < model.Meshes.Count; i++)
            {
                MtMesh mesh = model.Meshes[i];
                Assimp.Mesh iMesh = new Assimp.Mesh(mesh.Name, Assimp.PrimitiveType.Triangle);
                iMesh.UVComponentCount[0] = 2; // Required for some reason

                // MATERIAL
                iMesh.MaterialIndex = mesh.MaterialId.Value;

                // BONES - Assimp requires all of the bones even if they don't have weights for some reason
                for (int j = 0; j < model.Joints.Count; j++)
                {
                    iMesh.Bones.Add(new Assimp.Bone(model.Joints[j].Name, Assimp.Matrix3x3.Identity, new Assimp.VertexWeight[0]));
                }

                // VERTICES
                int currentVertex = 0;
                foreach (MtVertex vertex in mesh.Vertices)
                {
                    // Position
                    iMesh.Vertices.Add(new Assimp.Vector3D(vertex.AbsolutePosition.Value.X, vertex.AbsolutePosition.Value.Y, vertex.AbsolutePosition.Value.Z));

                    // UV
                    iMesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D(vertex.TextureCoordinates.Value.X, vertex.TextureCoordinates.Value.Y, 0));

                    // Weights
                    foreach (MtWeightPosition iweight in vertex.Weights)
                    {
                        Assimp.Bone bone = iMesh.Bones[iweight.JointIndex.Value];
                        float weight = iweight.Weight == 0 ? 1 : iweight.Weight.Value;
                        bone.VertexWeights.Add(new Assimp.VertexWeight(currentVertex, weight));
                    }

                    // Colors
                    if (vertex.ColorRGBA != null)
                    {
                        float R = vertex.ColorRGBA.Value.X;
                        float G = vertex.ColorRGBA.Value.Y;
                        float B = vertex.ColorRGBA.Value.Z;
                        float A = vertex.ColorRGBA.Value.W;
                        iMesh.VertexColorChannels[0].Add(new Assimp.Color4D(R, G, B, A));
                    }

                    // Normals
                    if (vertex.Normals != null)
                    {
                        iMesh.Normals.Add(new Assimp.Vector3D(vertex.Normals.Value.X, vertex.Normals.Value.Y, vertex.Normals.Value.Z));
                    }

                    currentVertex++;
                }

                // FACES
                foreach (MtFace face in mesh.Faces)
                {
                    iMesh.Faces.Add(new Assimp.Face(new int[] { face.VertexIndices[0], face.VertexIndices[1], face.VertexIndices[2] }));
                }

                assimpScene.Meshes.Add(iMesh);

                // MESH NODE
                Assimp.Node iMeshNode = new Assimp.Node("MeshNode" + i.ToString("D4"));
                iMeshNode.MeshIndices.Add(i);
                assimpScene.RootNode.Children.Add(iMeshNode);
            }

            // BONES (Node hierarchy)
            foreach (MtJoint joint in model.Joints)
            {
                Assimp.Node boneNode = new Assimp.Node(joint.Name);

                Assimp.Node parentNode;
                if (joint.ParentId == null || joint.ParentId == -1)
                {
                    parentNode = assimpScene.RootNode;
                }
                else
                {
                    parentNode = assimpScene.RootNode.FindNode(model.Joints[joint.ParentId.Value].Name);
                }

                boneNode.Transform = AssimpUtils.ToAssimp(joint.RelativeTransformationMatrix.Value);

                parentNode.Children.Add(boneNode);
            }

            return assimpScene;
        }

        // For maps with multiple models. Doesn't support skeletal data
        public static Assimp.Scene ExportScene(MtScene scene)
        {

            List<MtModel> models = new List<MtModel>();
            foreach (MtScene.MtModelNode modelNode in scene.Models)
            {
                models.Add(modelNode.Model);
            }

            if (models.Count == 1)
            {
                return ExportModel(scene);
            }

            Assimp.Scene assimpScene = AssimpUtils.GetBaseScene();

            for (int it = 0; it < models.Count; it++)
            {
                MtModel model = models[it];
                Assimp.Node modelNode = new Assimp.Node(model.Name);

                int baseMaterialCount = assimpScene.Materials.Count;

                // MATERIALS / TEXTURES
                for (int i = 0; i < model.Materials.Count; i++)
                {
                    Assimp.TextureSlot texture = new Assimp.TextureSlot();
                    string materialName = Path.GetFileNameWithoutExtension(model.Materials[i].DiffuseTextureFileName);
                    texture.FilePath = model.Name + "." + materialName;
                    texture.TextureType = Assimp.TextureType.Diffuse;

                    Assimp.Material mat = new Assimp.Material();
                    mat.Name = model.Name + "." + model.Materials[i].Name;
                    mat.TextureDiffuse = texture;
                    assimpScene.Materials.Add(mat);
                }

                // MESHES
                for (int i = 0; i < model.Meshes.Count; i++)
                {
                    MtMesh mesh = model.Meshes[i];
                    Assimp.Mesh iMesh = new Assimp.Mesh(model.Name + "." + mesh.Name, Assimp.PrimitiveType.Triangle);
                    iMesh.UVComponentCount[0] = 2; // Required for some reason

                    // MATERIAL
                    iMesh.MaterialIndex = baseMaterialCount + mesh.MaterialId.Value;

                    // BONES - Assimp requires all of the bones even if they don't have weights for some reason
                    //for (int j = 0; j < model.Joints.Count; j++)
                    //{
                    //    iMesh.Bones.Add(new Assimp.Bone(model.Joints[j].Name, Assimp.Matrix3x3.Identity, new Assimp.VertexWeight[0]));
                    //}

                    // VERTICES
                    int currentVertex = 0;
                    foreach (MtVertex vertex in mesh.Vertices)
                    {
                        // Position
                        iMesh.Vertices.Add(new Assimp.Vector3D(vertex.AbsolutePosition.Value.X, vertex.AbsolutePosition.Value.Y, vertex.AbsolutePosition.Value.Z));

                        // UV
                        iMesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D(vertex.TextureCoordinates.Value.X, vertex.TextureCoordinates.Value.Y, 0));

                        // Weights
                        //foreach (MtWeightPosition iweight in vertex.Weights)
                        //{
                        //    Assimp.Bone bone = iMesh.Bones[iweight.JointIndex.Value];
                        //    float weight = iweight.Weight == 0 ? 1 : iweight.Weight.Value;
                        //    bone.VertexWeights.Add(new Assimp.VertexWeight(currentVertex, weight));
                        //}

                        // Colors
                        if (vertex.ColorRGBA != null)
                        {
                            float R = vertex.ColorRGBA.Value.X;
                            float G = vertex.ColorRGBA.Value.Y;
                            float B = vertex.ColorRGBA.Value.Z;
                            float A = vertex.ColorRGBA.Value.W;
                            iMesh.VertexColorChannels[0].Add(new Assimp.Color4D(R, G, B, A));
                        }

                        // Normals
                        if (vertex.Normals != null)
                        {
                            iMesh.Normals.Add(new Assimp.Vector3D(vertex.Normals.Value.X, vertex.Normals.Value.Y, vertex.Normals.Value.Z));
                        }

                        currentVertex++;
                    }

                    // FACES
                    foreach (MtFace face in mesh.Faces)
                    {
                        if (face.Clockwise)
                        {
                            iMesh.Faces.Add(new Assimp.Face(new int[] { face.VertexIndices[0], face.VertexIndices[1], face.VertexIndices[2] }));
                        }
                        else
                        {
                            iMesh.Faces.Add(new Assimp.Face(new int[] { face.VertexIndices[0], face.VertexIndices[2], face.VertexIndices[1] }));
                        }
                    }

                    assimpScene.Meshes.Add(iMesh);

                    // MESH NODE
                    Assimp.Node iMeshNode = new Assimp.Node(model.Name + "." + mesh.Name);
                    iMeshNode.MeshIndices.Add(assimpScene.Meshes.Count - 1);
                    modelNode.Children.Add(iMeshNode);
                }

                assimpScene.RootNode.Children.Add(modelNode);
            }

            return assimpScene;
        }
    }
}
