
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HardCoded.VRigUnity.BVH {
    public class BVHExporter {
        private static Dictionary<HumanBodyBones, Transform> GetTransforms(Animator animator) {
			var dict = new Dictionary<HumanBodyBones, Transform>();
            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones))) {
				if (bone == HumanBodyBones.LastBone) {
					continue;
				}

				Transform tr = animator.GetBoneTransform(bone);
				if (tr != null) {
					dict.Add(bone, tr);
				}
			}

			return dict;
		}

		/// <summary>
		/// Recursive setup to get the bone node hierarchy
		/// </summary>
		private struct BoneNode {
			public HumanBodyBones bone;
			public Transform transform;
			public BoneNode[] children;

			public bool IsRoot => bone == HumanBodyBones.LastBone;
			public bool IsLeaf => children.Length == 0;
			public bool HasPosition => IsRoot || bone == HumanBodyBones.Hips;

			public BoneNode(HumanBodyBones bone, Dictionary<HumanBodyBones, Transform> dictionary) {
				this.bone = bone;
				transform = dictionary[bone];
				List<BoneNode> boneNodes = new();
				foreach (var entry in dictionary) {
					if (entry.Value.parent == transform) {
						boneNodes.Add(new BoneNode(entry.Key, dictionary));
					}
				}

				children = boneNodes.ToArray();
			}

			// Create a root bone node
			public BoneNode(Dictionary<HumanBodyBones, Transform> dictionary) {
				bone = HumanBodyBones.LastBone;
				transform = dictionary[HumanBodyBones.Hips].parent;
				children = new [] { new BoneNode(HumanBodyBones.Hips, dictionary) };
			}
		}

		private static Quaternion ShortestRotation(Quaternion a, Quaternion b) {
			if (Quaternion.Dot(a, b) < 0) {
				return a * Quaternion.Inverse(new Quaternion(-b.x, -b.y, -b.z, -b.w));
			}
	
			return a * Quaternion.Inverse(b);
		}

		public struct MocapData {
			public Vector3 pos;
			public Vector3 rot;

			public MocapData(Transform transform) {
				// unity mirror rotation and position on X axis
				pos = transform.localPosition;
				pos.x = -pos.x;
				
				// Quaternion parentRot = transform.parent.rotation;
				// parentRot.y = -parentRot.y;
				// parentRot.z = -parentRot.z;

				Quaternion preRot = transform.localRotation;
				preRot.y = -preRot.y;
				preRot.z = -preRot.z;

				rot = preRot.eulerAngles; //ShortestRotation(parentRot, preRot).eulerAngles;
				// rot = transform.localRotation.eulerAngles;
			}
		}

		public class MocapCollection {
			public List<KeyValuePair<long, Dictionary<HumanBodyBones, MocapData>>> timeline = new();
			
			public void Update(long timestamp, Animator animator) {
				var data = new Dictionary<HumanBodyBones, MocapData>();
				foreach (var entry in GetTransforms(animator)) {
					data.Add(entry.Key, new(entry.Value));
				}
				timeline.Add(new(timestamp, data));
			}
		}

		private static string FormatFloat(float value) {
			return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000000}", value);
		}

		private static int BuildOutput(StringBuilder builder, Dictionary<HumanBodyBones, int> index, BoneNode node, int depth, int offset = 0) {
			string ident = new('\t', depth);
			index.Add(node.bone, offset);
			
			if (depth == 0) {
				builder
					.Append(ident).Append("HIERARCHY\n")
					.Append(ident).Append("ROOT ")
				;
			} else {
				builder.Append(ident).Append("JOINT ");
			}

			Vector3 off = node.transform.localPosition;
			string offset_string = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:0.000000} {1:0.000000} {2:0.000000}", -off.x, off.y, off.z);
			builder
				.Append(node.transform == null ? "Root" : node.transform.name).Append("\n")
				.Append(ident).Append("{\n")
				.Append(ident).Append("\tOFFSET ").Append(offset_string).Append("\n")
			;

			if (node.HasPosition) {
				builder.Append(ident).Append("\tCHANNELS 6 Xposition Yposition Zposition Xrotation Yrotation Zrotation\n");
			} else {
				builder.Append(ident).Append("\tCHANNELS 3 Xrotation Yrotation Zrotation\n");
			}

			int variables = node.HasPosition ? 6 : 3;
			foreach (var child in node.children) {
				variables += BuildOutput(builder, index, child, depth + 1, variables + offset);
			}

			if (node.IsLeaf) {
				builder
					.Append(ident).Append("\tEnd Site\n")
					.Append(ident).Append("\t{\n")
					.Append(ident).Append("\t\tOFFSET ").Append(offset_string).Append("\n")
					.Append(ident).Append("\t}\n")
				;
			}
			
			builder.Append(ident).Append("}\n");
			return variables;
		}
		
		public static string GenerateData(Animator animator, MocapCollection collection) {
			var nodes = new BoneNode(GetTransforms(animator));

			// Clear the animator
			animator.WriteDefaultValues();

			var builder = new StringBuilder();
			var indexer = new Dictionary<HumanBodyBones, int>();
			var channels = BuildOutput(builder, indexer, nodes, 0);
			builder.Append("MOTION\n");
			builder.Append("Frames: ").Append(collection.timeline.Count).Append("\n");
			builder.Append("Frame Time: 0.02\n");
			
			string data = string.Join(" ", Enumerable.Repeat("0", channels));
			
			float[] array = new float[channels];
			foreach (var entry in collection.timeline) {
				var timestamp = entry.Key;
				var values = entry.Value;

				foreach (var mocap in values) {
					int offset = indexer[mocap.Key];
					var modata = mocap.Value;
					if (mocap.Key == HumanBodyBones.Hips) {
						array[offset    ] = modata.pos.x;
						array[offset + 1] = modata.pos.y;
						array[offset + 2] = modata.pos.z;
						offset += 3;
					}
					
					array[offset    ] = modata.rot.x;
					array[offset + 1] = modata.rot.y;
					array[offset + 2] = modata.rot.z;
				}

				string mocapString = string.Join(" ", array.Select(v => FormatFloat(v)));
				builder.Append(mocapString).Append("\n");
			}
			
			return builder.ToString();
		}

		public static void Test(Animator animator, MocapCollection collection) {
			string text = GenerateData(animator, collection);

			TextEditor te = new();
			te.text = text;
			te.SelectAll();
			te.Copy();
		}
	}
}