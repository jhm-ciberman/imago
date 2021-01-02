using Veldrid;

namespace LifeSim.Rendering
{
    public struct ShaderDescription
    {
        public struct Macro
        {
            public string name;
            public string value;
            public Macro(string name, string value) { this.name = name; this.value = value;}
            public Macro(string name)               { this.name = name; this.value = "";}
        }

        public string filename;
        public ResourceLayout? passResourcelayout;
        public ResourceLayout? materialResourcelayout;
        public ResourceLayout? objectResourcelayout;
        public VertexLayoutDescription[] vertexLayouts;
        public Macro[] macros;


        public ShaderDescription(
            string filename, 
            ResourceLayout passResourcelayout, 
            ResourceLayout materialResourcelayout, 
            ResourceLayout objectResourcelayout, 
            VertexLayoutDescription[] vertexLayouts,
            Macro[] macros
        )
        {
            this.filename = filename;
            this.passResourcelayout = passResourcelayout;
            this.materialResourcelayout = materialResourcelayout;
            this.objectResourcelayout = objectResourcelayout;
            this.vertexLayouts = vertexLayouts;
            this.macros = macros;
        }
    }
}