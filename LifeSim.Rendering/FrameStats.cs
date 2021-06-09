using System.Collections.Generic;
using System.Diagnostics;
using Veldrid;

namespace LifeSim.Rendering
{
    public class FrameProfiler
    {
        public class FrameStats
        {
            public int pipelineChanges;
            public int meshChanges;
            public int materialChanges;
            public int drawCalls;
            public int uniqueMaterials;
            public int uniqueMeshes;
            public long passTime;
        }

        public readonly FrameStats stats = new FrameStats();

        private HashSet<Material> _uniqueMaterials = new HashSet<Material>();
        private HashSet<Mesh> _uniqueMeshes = new HashSet<Mesh>();

        private Stopwatch _stopwatch = new Stopwatch();

        public void BeginFrame()
        {
            this._uniqueMaterials.Clear();
            this._uniqueMeshes.Clear();
            this.stats.pipelineChanges = 0;
            this.stats.meshChanges = 0;
            this.stats.materialChanges = 0;
            this.stats.drawCalls = 0;
            this._stopwatch.Restart();
        }

        public void ChangeMesh(Mesh mesh)
        {
            this._uniqueMeshes.Add(mesh);
            this.stats.meshChanges++;
        }

        public void ChangePipeline(Pipeline pipeline)
        {
            this.stats.pipelineChanges++;
        }

        public void ChangeMaterial(Material material)
        {
            this._uniqueMaterials.Add(material);
            this.stats.materialChanges++;
        }

        public void DrawCall()
        {
            this.stats.drawCalls++;
        }

        public void EndFrame()
        {
            this.stats.uniqueMaterials = this._uniqueMaterials.Count;
            this.stats.uniqueMeshes = this._uniqueMeshes.Count;
            this._stopwatch.Stop();
            this.stats.passTime = this._stopwatch.ElapsedTicks;
        }
    }
}