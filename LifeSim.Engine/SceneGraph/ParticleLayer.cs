using System.Collections.Generic;
using System.Numerics;
using LifeSim.Core;
using LifeSim.Rendering;

namespace LifeSim.Engine.SceneGraph 
{
    public class ParticleLayer
    {
        // particle is a struct
        private readonly Particle[] _particles = new Particle[1000];
        public IReadOnlyList<Particle> Particles => this._particles;

        public Texture Texture { get; set; }

        public ParticleLayer(Texture texture)
        {
            this.Texture = texture;
        }

        private Vector3 _gravity = new Vector3(0, -9.8f, 0);

        private int _lastUsedParticle = 0;

        private int _FindUnusedParticle()
        {
            // starting from last used particle, loop all particles
            for (int i = this._lastUsedParticle; i < this._particles.Length; i++) {
                // if particle is unused, return it
                if (this._particles[i].Life <= 0.0f) {
                    this._lastUsedParticle = i;
                    return i;
                }
            }

            for (int i = 0; i < this._lastUsedParticle; i++) {
                if (this._particles[i].Life <= 0.0f) {
                    this._lastUsedParticle = i;
                    return i;
                }
            }

            // if all particles are taken, override the first one (note that if it repeatedly hits this, then it will never be able to add more particles)
            this._lastUsedParticle = 0;
            return 0; 
        }

        public void AddParticle(Vector3 position, Vector3 velocity, float life, float size, Color color)
        {
            int particleIndex = this._FindUnusedParticle();
            this._particles[particleIndex] = new Particle(position, velocity, life, size, color);
        }

        public void Update(float deltaTime)
        {
            for (int i = 0; i < this._particles.Length; i++) {
                Particle particle = this._particles[i];

                particle.Position += particle.Velocity * deltaTime;
                particle.Velocity += this._gravity * deltaTime;
                particle.Life -= deltaTime;

                this._particles[i] = particle;
            }
        }
    }
}