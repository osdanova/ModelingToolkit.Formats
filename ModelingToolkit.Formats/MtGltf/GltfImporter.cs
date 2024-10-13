using ModelingToolkit.Core;
using SharpGLTF.Runtime;
using SharpGLTF.Schema2;

namespace ModelingToolkit.Formats.MtGltf
{
    public class GltfImporter
    {
        /*
         * > LogicalNodes
         * For each model there's a LogicalNode whose VisualParent = null. Blender calls these "Armature" if skinned or just a mesh.
         * Joint nodes are children of these nodes. They have SkinJoint as true.
         * Each mesh in the model is a child of this node. These nodes have a Mesh (LogicalMesh)
         * 
         * > LogicalMesh > Primitive[0] > Material > LogicalIndex
         * > LogicalMaterials > Channels > BaseColor > Texture > PrimaryImage
         */

        public static MtScene Import(string filePath)
        {
            MtScene scene = new MtScene();

            ModelRoot gltfModelRoot = ModelRoot.Load(filePath);
            SceneTemplate Template = SceneTemplate.Create(gltfModelRoot.DefaultScene);
            List<MeshPrimitive>[] Parts = new List<MeshPrimitive>[gltfModelRoot.LogicalMeshes.Count];

            // Skeleton
            // LogicalNodes[].Name .LocalMatrix .WorldMatrix .VisualParent(Node.LogicalIndex) .VisualChildren[]

            // LogicalNodes contains the list of joints and the list of meshes (With Local/WorldMatrix)
            // Bones do not have a mesh while mesh ones do

            // load the model's images
            //foreach (var logicalImage in model.LogicalImages)
            //{
            //    using var stream = new MemoryStream(logicalImage.Content.Content.ToArray());
            //    var img = new Image(stream);
            //    images.Add(logicalImage.Content, img);
            //}


            // Nodes
            // Get Root Nodes (Visual parent = null)
            // Get mesh nodes (Visual parent = root node + Mesh != null)

            Dictionary<Node, MtModel> models = new Dictionary<Node, MtModel>();
            Dictionary<Node, List<Node>> modelMeshNodes = new Dictionary<Node, List<Node>>();
            Dictionary<Node, Node> modelRootBoneNodes = new Dictionary<Node, Node>();

            // Root/Model nodes
            foreach (Node logicalNode in gltfModelRoot.LogicalNodes)
            {
                if (logicalNode.VisualParent == null)
                {
                    MtModel model = new MtModel();
                    model.Name = logicalNode.Name;

                    models.Add(logicalNode, model);
                    modelMeshNodes.Add(logicalNode, new List<Node>());
                    modelRootBoneNodes.Add(logicalNode, null);
                }
            }

            // Mesh and root bone Nodes
            foreach (Node logicalNode in gltfModelRoot.LogicalNodes)
            {
                if (logicalNode.VisualParent != null && models.ContainsKey(logicalNode.VisualParent))
                {
                    // Mesh
                    if(logicalNode.Mesh != null)
                    {
                        modelMeshNodes[logicalNode.VisualParent].Add(logicalNode);
                    }
                    // Root bone
                    else if (logicalNode.IsSkinJoint)
                    {
                        modelRootBoneNodes[logicalNode.VisualParent] = logicalNode;
                    }
                }
            }

            MtMaterial[] Materials = new MtMaterial[gltfModelRoot.LogicalMaterials.Count];
            MtMaterial[] Images = new MtMaterial[gltfModelRoot.LogicalImages.Count]; // .Content.SourcePath > Absolute path to img file

            List<MtVertex> Vertices = [];
            List<int> Indices = [];

            foreach(var mat in gltfModelRoot.LogicalMaterials)
            {
                foreach(var matCh in mat.Channels) // Channel BaseColor
                {
                    var tex = matCh.Texture; // .PrimaryImage
                }
            }

            // create vertex/index array
            for (int i = 0; i < gltfModelRoot.LogicalMeshes.Count; i++)
            {
                var mesh = gltfModelRoot.LogicalMeshes[i];
                var vertexCount = Vertices.Count;
                var indexStart = Indices.Count;
                var part = Parts[i] = new();

                foreach (var primitive in mesh.Primitives)
                {
                    var verts = primitive.GetVertexAccessor("POSITION").AsVector3Array(); //XYZ pos
                    var uvs = primitive.GetVertexAccessor("TEXCOORD_0").AsVector2Array(); // UVs (0-1)
                    var normals = primitive.GetVertexAccessor("NORMAL").AsVector3Array(); //XYZ
                    var weights = primitive.GetVertexAccessor("WEIGHTS_0"); // Float4 WXYZ
                    var joints = primitive.GetVertexAccessor("JOINTS_0"); // Ubyte4 WXYZ

                    // not all primitives have weights/joints
                    //if (weights != null && joints != null)
                    //{
                    //    var ws = weights.AsVector4Array();
                    //    var js = joints.AsVector4Array();
                    //
                    //    for (int j = 0; j < verts.Count; j++)
                    //    {
                    //        Vertices.Add(new Vertex(verts[j], uvs[j], Vec3.One, normals[j], js[j], ws[j]));
                    //    }
                    //}
                    //else
                    //{
                    //    for (int j = 0; j < verts.Count; j++)
                    //    {
                    //        Vertices.Add(new Vertex(verts[j], uvs[j], Vec3.One, normals[j], new(), Vec4.One));
                    //    }
                    //}
                    //
                    //foreach (var index in primitive.GetIndices())
                    //    Indices.Add(vertexCount + (int)index);
                    //
                    //part.Add(new()
                    //{
                    //    Material = primitive.Material?.LogicalIndex ?? 0,
                    //    Index = indexStart,
                    //    Count = Indices.Count - indexStart
                    //});
                }
            }


            

            // TODO

            return scene;
        }
    }
}
