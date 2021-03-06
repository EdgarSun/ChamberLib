﻿using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using System.IO;
using ChamberLib.Content;

namespace ChamberLib.OpenTK
{
    public class Model : IModel
    {
        public Model(Content.ModelContent modelContent, Renderer renderer, IContentProcessor processor)
        {
            Renderer = renderer;

            var resolver = new ContentResolver();

            foreach (var ib in modelContent.IndexBuffers)
            {
                var ib2 = new IndexBuffer(ib);
                resolver.Add(ib, ib2);
                this.IndexBuffers.Add(ib2);
            }
            foreach (var vb in modelContent.VertexBuffers)
            {
                var vb2 = VertexBuffer.FromArray(vb.Vertices);
                resolver.Add(vb, vb2);
                this.VertexBuffers.Add(vb2);
            }
            foreach (var bone in modelContent.Bones)
            {
                var b2 = new Bone(bone);
                resolver.Add(bone, b2);
                this.Bones.Add(b2);
            }
            this.RootBone = this.Bones[modelContent.RootBoneIndex];
            int i;
            for (i = 0; i < this.Bones.Count; i++)
            {
                this.Bones[i].Index = i;
                foreach (var childIndex in modelContent.Bones[i].ChildBoneIndexes)
                {
                    this.Bones[i].Children.Add(this.Bones[childIndex]);
                }
            }
            foreach (var mesh in modelContent.Meshes)
            {
                foreach (var part in mesh.Parts)
                {
                    if (!resolver.Materials.ContainsKey(part.Material))
                    {
                        var mat2 = new Material(part.Material, resolver, processor);
                        resolver.Materials.Add(part.Material, mat2);
                    }
                }
            }
            foreach (var mesh in modelContent.Meshes)
            {
                this.Meshes.Add(new Mesh(mesh, resolver));
            }
            this.Tag = modelContent.AnimationData;
            this.Filename = modelContent.Filename;
        }

        public readonly Renderer Renderer;

        #region IModel implementation

        public System.Collections.Generic.IEnumerable<IMesh> GetMeshes()
        {
            return new IMesh[0];
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            if (!IsReady)
            {
                MakeReady();
            }

            foreach (var mesh in Meshes)
            {
                mesh.Draw(Renderer, world, view, projection, Lighting);
            }
        }

        public void EnableDefaultLighting()
        {
        }

        LightingData Lighting;
        public void SetAmbientLightColor(Vector3 value)
        {
            Lighting.AmbientLightColor = value;
        }

        public void SetEmissiveColor(Vector3 value)
        {
            Lighting.EmissiveColor = value;
        }

        public void SetDirectionalLight(DirectionalLight light, int index = 0)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException("index");

            Lighting.DirectionalLight = light;
        }

        public void DisableDirectionalLight(int index)
        {
            if (index == 0)
                throw new ArgumentOutOfRangeException("index");
        }

        public IEnumerable<Material> GetAllMaterials()
        {
            var materials = new HashSet<Material>(Meshes.SelectMany(m => m.Parts).Select(p => p.Material));
            foreach (var material in materials)
            {
                yield return material;
            }
        }

        public void SetAlpha(float alpha)
        {
            foreach (var material in GetAllMaterials())
            {
                material.Alpha = alpha;
            }
        }

        public void SetTexture(ITexture2D texture)
        {
            foreach (var material in GetAllMaterials())
            {
                material.Texture = texture;
            }
        }

        public void SetWorldViewProjection(Matrix transform, Matrix view, Matrix projection)
        {
        }

        public void SetBoneTransforms(Matrix[] boneTransforms, float verticalOffset, Matrix world)
        {
            foreach (var material in GetAllMaterials())
            {
                if (material.Shader == BuiltinShaders.SkinnedShader)
                {
                    int i;
                    for (i = 0; i < 30 && i < boneTransforms.Length; i++)
                    {
                        var name = string.Format("bones[{0}]", i);
                        BuiltinShaders.SkinnedShader.SetUniform(name, boneTransforms[i]);
                    }

                    break;
                }
            }
        }

        public object Tag { get; set; }

        public IBone Root
        {
            get
            {
                return RootBone;
            }
            set
            {
                RootBone = (Bone)value;
            }
        }

        #endregion

        public List<Mesh> Meshes = new List<Mesh>();
        public List<Bone> Bones = new List<Bone>();
        public Bone RootBone;

        public List<IndexBuffer> IndexBuffers = new List<IndexBuffer>();
        public List<VertexBuffer> VertexBuffers = new List<VertexBuffer>();

        bool IsReady = false;
        void MakeReady()
        {
            if (IsReady) return;

            foreach (var mesh in Meshes)
            {
                mesh.MakeReady();
            }

            IsReady = true;
        }
        public string Filename;
    }
}

