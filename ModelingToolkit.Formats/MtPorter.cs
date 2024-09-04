using ModelingToolkit.Core;
using SharpGLTF.Scenes;
using System.Drawing.Imaging;

namespace ModelingToolkit.Formats
{
    public class MtPorter
    {
        public static MtScene ImportScene(string filepath)
        {
            string extension = Path.GetExtension(filepath).ToLower();
            if(extension == ".fbx")
            {
                return MtAssimp.AssimpImporter.ImportScene(filepath);
            }
            // TODO
            //else if(extension == ".gltf" || extension == ".glb")
            //{
            //    return MtGltf.GltfImporter.Import(new byte[0]);
            //}
            else
            {
                throw new Exception("Invalid file type");
            }
        }

        public static void ExportScene(MtScene scene, string filepath, FileType fileType)
        {
            if (fileType == FileType.FBX)
            {
                Assimp.Scene assimpScene = MtAssimp.AssimpExporter.ExportScene(scene);
                Assimp.AssimpContext context = new Assimp.AssimpContext();
                context.ExportFile(assimpScene, filepath, "fbx");
            }
            else if (fileType == FileType.GLB)
            {
                SceneBuilder sb = MtGltf.GltfExporter.CreateScene(scene);
                sb.ToGltf2().SaveGLTF(filepath);
            }
            else if (fileType == FileType.GLTF)
            {
                SceneBuilder sb = MtGltf.GltfExporter.CreateScene(scene);
                sb.ToGltf2().SaveGLB(filepath);
            }
            else
            {
                throw new Exception("Invalid file type");
            }
        }

        /*
         * Currently supported file types
         */
        public enum FileType
        {
            FBX,
            GLB,
            GLTF
        }

        /*
         * Exports the textures from the given model to the given model filepath
         */
        public void ExportTextures(string filePath, MtModel model, string prefix = "")
        {
            for (int i = 0; i < model.Materials.Count; i++)
            {
                string textureFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(model.Materials[i].DiffuseTextureFileName) + ".png");
                model.Materials[i].DiffuseTextureBitmap.Save(textureFilePath, ImageFormat.Png);
            }
        }
    }
}
