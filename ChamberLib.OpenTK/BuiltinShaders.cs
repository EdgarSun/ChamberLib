﻿using System;
using System.IO;
using ChamberLib.Content;

namespace ChamberLib.OpenTK
{
    public static class BuiltinShaders
    {
        static ShaderAdapter basicShader;
        public static ShaderAdapter BasicShader
        {
            get
            {
                if (basicShader == null)
                {
                    basicShader =
                        new ShaderAdapter(
                            BasicShaderContent,
                            new [] {
                                "in_position",
                                "in_normal",
                                "in_texture_coords"
                            });
                }

                return basicShader;
            }
        }
        static ShaderContent basicShaderContent;
        public static ShaderContent BasicShaderContent
        {
            get
            {
                if (basicShaderContent == null)
                {
                    basicShaderContent =
                        new ShaderContent(
                            BasicShaderVert,
                            BasicShaderFrag,
                            "$basic");
                }

                return basicShaderContent;
            }
        }

        public static readonly string BasicShaderVert = @"
#version 140

precision highp float;

uniform mat4 worldViewProj;
uniform mat4 world;
uniform mat4 worldView;

in vec3 in_position;
in vec3 in_normal;
in vec2 in_texture_coords;

out vec3 vf_normal_ws;
out vec2 vf_texture_coords;
out vec3 vf_position_ws;

void main(void)
{
    vec3 position = in_position;
    vec4 transformed = worldViewProj * vec4(position, 1);

    vf_position_ws = (world * vec4(position, 0)).xyz;

    vf_normal_ws = (world * vec4(in_normal, 0)).xyz;

    vf_texture_coords = in_texture_coords;

    gl_Position = transformed;
}
";

        public static readonly string BasicShaderFrag = @"
#version 140

precision highp float;

uniform mat4 view;
uniform sampler2D material_texture0;
uniform bool use_texture;
uniform vec3 material_diffuse_color = vec3(1, 1, 1);
uniform vec3 material_emissive_color = vec3(0, 0, 0);
uniform vec3 material_specular_color = vec3(0, 0, 0);
uniform float material_specular_power = 0;
uniform float material_alpha = 1;
uniform vec3 light_ambient = vec3(0.2, 0.2, 0.2);
uniform vec3 light_direction_ws;
uniform vec3 light_diffuse_color = vec3(1, 1, 1);
uniform vec3 light_specular_color = vec3(1, 1, 1);
uniform vec3 camera_position_ws;

in vec3 vf_normal_ws;
in vec2 vf_texture_coords;
in vec3 vf_position_ws;

out vec4 out_frag_color;

void main(void)
{
    vec3 diffuse = material_diffuse_color;
    float alpha = material_alpha;

    if (use_texture)
    {
        vec4 tex_value = texture(material_texture0, vf_texture_coords);
        diffuse *= tex_value.rgb;
        alpha *= tex_value.a;
    }

    vec3 normal_ws = normalize(vf_normal_ws);

    float costheta = clamp(-dot(normal_ws, light_direction_ws), 0, 1);
    float zeroL = step(0, costheta);

    vec3 fragment_to_light_direction_ws = -light_direction_ws;
    vec3 fragment_to_camera_direction_ws = normalize(camera_position_ws - vf_position_ws);
    vec3 half_vector = normalize(fragment_to_camera_direction_ws + fragment_to_light_direction_ws);

    float specular_factor = max(pow(clamp(dot(normal_ws, half_vector), 0.0, 1.0), material_specular_power),0);

    vec3 color =
        diffuse * (light_diffuse_color*costheta + light_ambient + material_emissive_color) +
        specular_factor * light_specular_color * material_specular_color;

    out_frag_color = vec4(color, alpha);
}
";

        static ShaderAdapter skinnedShader;
        public static ShaderAdapter SkinnedShader
        {
            get
            {
                if (skinnedShader == null)
                {
                    skinnedShader =
                        new ShaderAdapter(
                            SkinnedShaderContent,
                            new [] {
                                "in_position",
                                "in_normal",
                                "in_texture_coords",
                                "in_blend_indices",
                                "in_blend_weights",
                            });
                }

                return skinnedShader;
            }
        }
        static ShaderContent skinnedShaderContent;
        public static ShaderContent SkinnedShaderContent
        {
            get
            {
                if (skinnedShaderContent == null)
                {
                    skinnedShaderContent =
                        new ShaderContent(
                            SkinnedShaderVert,
                            SkinnedShaderFrag,
                            "$skinned");
                }

                return skinnedShaderContent;
            }
        }
        public static readonly string SkinnedShaderVert = @"
#version 140

precision highp float;

uniform mat4 worldViewProj;
uniform mat4 world;

uniform mat4 bones[30];

in vec3 in_position;
in vec3 in_normal;
in vec2 in_texture_coords;
in vec4 in_blend_indices;
in vec4 in_blend_weights;

out vec3 vf_normal_ws;
out vec2 vf_texture_coords;
out vec3 vf_position_ws;

void main(void)
{
    mat4 blend =
        bones[int(in_blend_indices.x)] * in_blend_weights.x +
        bones[int(in_blend_indices.y)] * in_blend_weights.y +
        bones[int(in_blend_indices.z)] * in_blend_weights.z +
        bones[int(in_blend_indices.w)] * in_blend_weights.w;

    mat3x4 blend2;
    blend2[0] = blend[0];
    blend2[1] = blend[1];
    blend2[2] = blend[2];

    vec3 skinned = (blend * vec4(in_position,1)).xyz;

    vec4 transformed = worldViewProj * vec4(skinned, 1);

    vf_position_ws = (world * vec4(in_position, 0)).xyz;

    vf_normal_ws = (world * vec4(in_normal, 0)).xyz;

    vf_texture_coords = in_texture_coords;

    gl_Position = transformed;
}";

        public static readonly string SkinnedShaderFrag = BasicShaderFrag;

    }
}

