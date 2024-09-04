using System.Numerics;

namespace ModelingToolkit.Formats.MtAssimp
{
    public class AssimpUtils
    {
        public static Assimp.Matrix4x4 ToAssimp(Matrix4x4 m) => new Assimp.Matrix4x4(m.M11, m.M21, m.M31, m.M41, m.M12, m.M22, m.M32, m.M42, m.M13, m.M23, m.M33, m.M43, m.M14, m.M24, m.M34, m.M44);
        public static Matrix4x4 ToNumerics(Assimp.Matrix4x4 m) => new Matrix4x4(m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2, m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);
        public static Assimp.Vector3D ToAssimp(Vector3 v) => new Assimp.Vector3D(v.X, v.Y, v.Z);
        public static Vector3 ToNumerics(Assimp.Vector3D v) => new Vector3(v.X, v.Y, v.Z);
        public static Assimp.Color4D ToAssimpColor(Vector4 c) => new Assimp.Color4D(c.X, c.Y, c.Z, c.W);
        public static Vector4 ToNumericsColor(Assimp.Color4D c) => new Vector4(c.R, c.G, c.B, c.A);

        public enum FileFormat
        {
            // Common
            collada,
            fbx,
            fbxa,
            obj,

            // Other
            x,
            stp,
            objnomtl,
            stl,
            stlb,
            ply,
            plyb,
            //3ds,
            gltf2,
            glb2,
            gltf,
            glb,
            assbin,
            assxml,
            x3d,
            m3d,
            m3da,
            //3mf,
            pbrt,
            assjson
        }

        // Returns the list of bones in the given Assimp scene
        public static List<Assimp.Node> GetNodeList(Assimp.Scene scene)
        {
            List<Assimp.Node> nodeList = new List<Assimp.Node>();
            GetNodeAndChildren(scene.RootNode, nodeList);
            return nodeList;
        }
        public static void GetNodeAndChildren(Assimp.Node node, List<Assimp.Node> nodeList)
        {
            // Exclude mesh nodes
            if (node.HasMeshes)
            {
                return;
            }

            // Exclude Rootnode
            if (node.Name != "RootNode")
            {
                nodeList.Add(node);
            }

            // Recursively process child nodes
            for (int i = 0; i < node.ChildCount; i++)
            {
                GetNodeAndChildren(node.Children[i], nodeList);
            }
        }

        // Returns a basic Assimp scene. It also contains notes on how to make a valid Assimp scene for exporting
        public static Assimp.Scene GetBaseScene()
        {
            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("RootNode");

            return scene;

            /* To get a valid assimp scene you need the following:
             * A mesh.
             * A mesh node. One per mesh.
             * Material + texture for the mesh. (Optional)
             * Bones (Weights) for the mesh. Note that Assimp requires each mesh to have all bones even if they have no vertex weights (Optional)
             * Vertices for the mesh.
             * Faces for the mesh.
             * Nodes (Skeleton) as children of rootnode. One per bone. Same name as the bones.
             */
        }

        // Finds a bone by name in a bone list
        public static Assimp.Bone FindBone(List<Assimp.Bone> list_bones, string boneName)
        {
            foreach (Assimp.Bone bone in list_bones)
            {
                if (bone.Name == boneName)
                    return bone;
            }

            return null;
        }

        // Returns the file extension that the given format uses
        public static string GetFormatFileExtension(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.collada:
                    return "dae";
                case FileFormat.fbx:
                case FileFormat.fbxa:
                    return "fbx";
                default:
                    return format.ToString();
            }
        }

        // Exports the given Assimp scene in the given format
        public static void ExportScene(Assimp.Scene scene, FileFormat format = FileFormat.fbx, string filename = "fileout.fbx")
        {
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.ExportFile(scene, filename, format.ToString());
        }
    }
}
