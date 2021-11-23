using System;
using System.Collections.Generic;
using System.Numerics;
using LifeSim.Engine.Rendering;

namespace LifeSim.Engine.SceneGraph
{
    public abstract class ParticleSystem
    {
        protected readonly SwapPopList<Particle> _particles = new SwapPopList<Particle>();
        public IReadOnlyList<Particle> Particles => this._particles;

        public Texture Texture { get; set; }

        public ParticleSystem(Texture texture)
        {
            this.Texture = texture;
        }

        public abstract void Update(float deltaTime);

        public abstract void Spawn(Vector3 position);

        public void Render(Renderer renderer, ICamera camera)
        {
            this._SortParticles(camera.Position);
            renderer.ParticlesRenderer.Render(this._particles, this.Texture, camera);
        }

        private void _SortParticles(Vector3 cameraPosition)
        {
            for (int i = 0; i < this._particles.Count; i++)
            {
                var p = this._particles[i];
                p.DistanceToCamera = Vector3.Distance(this._particles[i].Position, cameraPosition);
                this._particles[i] = p;
            }

            this._particles.Sort((a, b) => b.DistanceToCamera.CompareTo(a.DistanceToCamera));

            if (Input.GetKeyDown(Veldrid.Key.T))
            {
                Console.WriteLine("PARTICLE START");
                foreach (var p in this._particles)
                {
                    Console.WriteLine(p.DistanceToCamera);
                }
            }
        }
    }
}