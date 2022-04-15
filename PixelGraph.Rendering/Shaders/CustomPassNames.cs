namespace PixelGraph.Rendering.Shaders
{
    public static class CustomPassNames
    {
        public const string Sky_ERP = "Sky_ERP";
        public const string SkyFinal_Cube = "SkyFinal_Cube";
        public const string SkyFinal_ERP = "SkyFinal_ERP";
        public const string SkyIrradiance = "SkyIrradiance";
        
        public const string Diffuse = "Diffuse";
        public const string DiffuseOIT = "DiffuseOIT";
        
        public const string Normals = "Normals";
        public const string NormalsOIT = "NormalsOIT";

        public const string PbrFilament = "PhysicsBasedRenderingFilament";
        public const string PbrFilamentOIT = "MeshPhysicsBasedFilamentOIT";

        public const string PbrJessie = "PhysicsBasedRenderingJessie";
        public const string PbrJessieOIT = "MeshPhysicsBasedJessieOIT";

        public const string PbrNull = "PhysicsBasedRenderingNull";
        public const string PbrNullOIT = "MeshPhysicsBasedNullOIT";

        public const string Occlusion = "OcclusionGeneration";
    }
}
