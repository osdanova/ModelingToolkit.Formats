using ModelingToolkit.Core;

namespace ModelingToolkit.Formats.MtAssimp
{
    public class AssimpImporter
    {
        public static MtScene ImportScene(string filePath)
        {
            string directory = Path.GetDirectoryName(filePath);

            Assimp.AssimpContext assimp = new Assimp.AssimpContext();
            Assimp.Scene assimpScene = assimp.ImportFile(filePath);

            MtModel model = new MtModel();

            // SKELETON
            List<Assimp.Node> nodeList = AssimpUtils.GetNodeList(assimpScene);

            // Get joint data. Assimp stores it in relative-to-parent format
            List<MtJoint> joints = new List<MtJoint>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                Assimp.Node node = nodeList[i];
                MtJoint joint = new MtJoint();
                joint.Name = node.Name;
                joint.RelativeTransformationMatrix = AssimpUtils.ToNumerics(node.Transform);
                if (node.Parent != null && nodeList.IndexOf(node.Parent) != -1)
                {
                    joint.ParentId = nodeList.IndexOf(node.Parent);
                }
                joint.Decompose();
                joints.Add(joint);
            }

            // Calculate absolute data as well
            model.Joints = joints;
            model.CalculateJointAbsoluteMatrices();

            // MATERIALS / TEXTURES
            for (int i = 0; i < assimpScene.Materials.Count; i++)
            {
                MtMaterial material = new MtMaterial();
                material.Name = assimpScene.Materials[i].Name;
                string textureFilepath = assimpScene.Materials[i].TextureDiffuse.FilePath;
                string[] pathSplit = textureFilepath.Split("\\");
                textureFilepath = pathSplit[pathSplit.Length - 1];
                textureFilepath = Path.Combine(directory, textureFilepath);
                textureFilepath += ".png";

                material.DiffuseTextureFileName = textureFilepath;
                material.DiffuseTextureBitmap = new System.Drawing.Bitmap(textureFilepath);

                model.Materials.Add(material);
            }

            // MESHES
            for (int i = 0; i < assimpScene.Meshes.Count; i++)
            {
                MtMesh mesh = new MtMesh();
                mesh.Name = assimpScene.Meshes[i].Name;
                mesh.MaterialId = assimpScene.Meshes[i].MaterialIndex;

                // Vertices
                for (int j = 0; j < assimpScene.Meshes[i].Vertices.Count; j++)
                {
                    MtVertex vertex = new MtVertex();

                    // Position
                    vertex.AbsolutePosition = AssimpUtils.ToNumerics(assimpScene.Meshes[i].Vertices[j]);
                    // UV
                    if (assimpScene.Meshes[i].TextureCoordinateChannels[0].Count > 0)
                    {
                        vertex.TextureCoordinates = AssimpUtils.ToNumerics(assimpScene.Meshes[i].TextureCoordinateChannels[0][j]);
                    }
                    // Color
                    if (assimpScene.Meshes[i].VertexColorChannels[0].Count > 0)
                    {
                        vertex.ColorRGBA = AssimpUtils.ToNumericsColor(assimpScene.Meshes[i].VertexColorChannels[0][j]);
                    }
                    // Normal
                    if (assimpScene.Meshes[i].Normals.Count > 0)
                    {
                        vertex.Normals = AssimpUtils.ToNumerics(assimpScene.Meshes[i].Normals[j]);
                    }

                    mesh.Vertices.Add(vertex);
                }
                // Weights
                for (int j = 0; j < assimpScene.Meshes[i].Bones.Count; j++)
                {
                    Assimp.Bone bone = assimpScene.Meshes[i].Bones[j];

                    int boneIndex = -1;
                    for (int k = 0; k < model.Joints.Count; k++)
                    {
                        if (model.Joints[k].Name == bone.Name)
                        {
                            boneIndex = k;
                            break;
                        }
                    }

                    for (int k = 0; k < bone.VertexWeights.Count; k++)
                    {
                        Assimp.VertexWeight weight = bone.VertexWeights[k];

                        MtWeightPosition weightPosition = new MtWeightPosition();
                        weightPosition.JointIndex = boneIndex;
                        weightPosition.Weight = weight.Weight;

                        mesh.Vertices[weight.VertexID].Weights.Add(weightPosition);
                    }
                }

                // Faces
                for (int j = 0; j < assimpScene.Meshes[i].Faces.Count; j++)
                {
                    Assimp.Face iFace = assimpScene.Meshes[i].Faces[j];
                    if (iFace.Indices.Count > 3)
                    {
                        throw new System.Exception("Sorry! We only accept triangles and you have a face with more than 3 vertices!");
                    }
                    else if (iFace.Indices.Count < 3)
                    {
                        throw new System.Exception("It seems like one of your faces consist of less than 3 vertices. Please make sure all faces have 3 vertices.");
                    }

                    MtFace face = new MtFace();
                    face.VertexIndices = iFace.Indices;
                    face.Clockwise = true;

                    mesh.Faces.Add(face);
                }

                mesh.BuildTriangleStrips();
                model.Meshes.Add(mesh);
            }

            MtScene scene = new MtScene();
            MtScene.MtModelNode modelNode = new MtScene.MtModelNode();
            modelNode.Model = model;
            scene.Models.Add(modelNode);

            return scene;
        }
    }
}
