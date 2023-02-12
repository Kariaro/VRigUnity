using UnityEngine;

namespace HardCoded.VRigUnity {
	public class BoneSettings {
		public const int FACE = 0;
		public const int NECK = 1;
		public const int LEFT_ARM = 2;
		public const int LEFT_WRIST = 3;
		public const int LEFT_FINGERS = 4;
		public const int RIGHT_ARM = 5;
		public const int RIGHT_WRIST = 6;
		public const int RIGHT_FINGERS = 7;
		public const int CHEST = 8;
		public const int HIPS = 9;
		public const int LEFT_LEG = 10;
		public const int LEFT_ANKLE = 11;
		public const int RIGHT_LEG = 12;
		public const int RIGHT_ANKLE = 13;
		public const int Count = RIGHT_ANKLE;

		// 16 bits set
		public const int Default = ((1 << (Count + 2)) - 1) & ~(
			(1 << LEFT_LEG) | (1 << LEFT_ANKLE) |
			(1 << RIGHT_LEG) | (1 << RIGHT_ANKLE)
		);

		public static bool Get(int index) {
			return (Settings.BoneMask & (1 << index)) != 0;
		}

		public static void Set(int index, bool set) {
			if (set) {
				Settings.BoneMask |= 1 << index;
			} else {
				Settings.BoneMask &= ~(1 << index);
			}
		}
		
		private static readonly HumanBodyBones[] FACE_BONES = new [] {
			HumanBodyBones.LeftEye,
			HumanBodyBones.RightEye
		};

		private static readonly HumanBodyBones[] NECK_BONES = new [] {
			HumanBodyBones.Neck,
			HumanBodyBones.Head,
			HumanBodyBones.Jaw
		};

		private static readonly HumanBodyBones[] LEFT_ARM_BONES = new [] {
			HumanBodyBones.LeftShoulder, // Part of group
			HumanBodyBones.LeftUpperArm,
			HumanBodyBones.LeftLowerArm
		};
		private static readonly HumanBodyBones[] LEFT_WRIST_BONES = new [] { HumanBodyBones.LeftHand };
		private static readonly HumanBodyBones[] LEFT_FINGERS_BONES = new [] {
			HumanBodyBones.LeftIndexProximal,
			HumanBodyBones.LeftIndexIntermediate,
			HumanBodyBones.LeftIndexDistal,
			HumanBodyBones.LeftMiddleProximal,
			HumanBodyBones.LeftMiddleIntermediate,
			HumanBodyBones.LeftMiddleDistal,
			HumanBodyBones.LeftRingProximal,
			HumanBodyBones.LeftRingIntermediate,
			HumanBodyBones.LeftRingDistal,
			HumanBodyBones.LeftLittleProximal,
			HumanBodyBones.LeftLittleIntermediate,
			HumanBodyBones.LeftLittleDistal,
			HumanBodyBones.LeftThumbProximal,
			HumanBodyBones.LeftThumbIntermediate,
			HumanBodyBones.LeftThumbDistal
		};
		
		private static readonly HumanBodyBones[] RIGHT_ARM_BONES = new [] {
			HumanBodyBones.RightShoulder, // Part of group
			HumanBodyBones.RightUpperArm,
			HumanBodyBones.RightLowerArm
		};
		private static readonly HumanBodyBones[] RIGHT_WRIST_BONES = new [] { HumanBodyBones.RightHand };
		private static readonly HumanBodyBones[] RIGHT_FINGERS_BONES = new [] {
			HumanBodyBones.RightIndexProximal,
			HumanBodyBones.RightIndexIntermediate,
			HumanBodyBones.RightIndexDistal,
			HumanBodyBones.RightMiddleProximal,
			HumanBodyBones.RightMiddleIntermediate,
			HumanBodyBones.RightMiddleDistal,
			HumanBodyBones.RightRingProximal,
			HumanBodyBones.RightRingIntermediate,
			HumanBodyBones.RightRingDistal,
			HumanBodyBones.RightLittleProximal,
			HumanBodyBones.RightLittleIntermediate,
			HumanBodyBones.RightLittleDistal,
			HumanBodyBones.RightThumbProximal,
			HumanBodyBones.RightThumbIntermediate,
			HumanBodyBones.RightThumbDistal
		};
		
		private static readonly HumanBodyBones[] CHEST_BONES = new [] {
			HumanBodyBones.Chest,
			HumanBodyBones.UpperChest,
			HumanBodyBones.Spine
		};

		private static readonly HumanBodyBones[] HIPS_BONES = new [] { HumanBodyBones.Hips };
		private static readonly HumanBodyBones[] LEFT_LEG_BONES = new [] { HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg };
		private static readonly HumanBodyBones[] LEFT_ANKLE_BONES = new [] { HumanBodyBones.LeftFoot, HumanBodyBones.LeftToes };
		private static readonly HumanBodyBones[] RIGHT_LEG_BONES = new [] { HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg };
		private static readonly HumanBodyBones[] RIGHT_ANKLE_BONES = new [] { HumanBodyBones.RightFoot, HumanBodyBones.RightToes };
		private static readonly HumanBodyBones[] NONE_BONES = new HumanBodyBones[0];

		public static HumanBodyBones[] GetBones(int index) {
			return index switch {
				FACE => FACE_BONES,
				NECK => NECK_BONES,
				LEFT_ARM => LEFT_ARM_BONES,
				LEFT_WRIST => LEFT_WRIST_BONES,
				LEFT_FINGERS => LEFT_FINGERS_BONES,
				RIGHT_ARM => RIGHT_ARM_BONES,
				RIGHT_WRIST => RIGHT_WRIST_BONES,
				RIGHT_FINGERS => RIGHT_FINGERS_BONES,
				CHEST => CHEST_BONES,
				HIPS => HIPS_BONES,
				LEFT_LEG => LEFT_LEG_BONES,
				LEFT_ANKLE => LEFT_ANKLE_BONES,
				RIGHT_LEG => RIGHT_LEG_BONES,
				RIGHT_ANKLE => RIGHT_ANKLE_BONES,
				_ => NONE_BONES
			};
		}

		public static bool CanExternalModify(HumanBodyBones bone) {
			for (int i = 0; i < Count; i++) {
				// If the bone is not disabled we do not allow this
				if (Get(i)) {
					HumanBodyBones[] array = GetBones(i);
					foreach (HumanBodyBones item in array) {
						if (bone == item) {
							// The bone belonged to an array that is controlled by our software
							return false;
						}
					}
				}
			}

			return true;
		}

		public static readonly Quaternion DefaultLeftArm = Quaternion.Euler(0, 0, 80);
		public static readonly Quaternion DefaultRightArm = Quaternion.Euler(0, 0, -80);
		private static readonly Quaternion DefaultRot = Quaternion.identity;
		public static Quaternion GetDefaultRotation(HumanBodyBones bone) {
			return bone switch {
				HumanBodyBones.LeftHand => DefaultLeftArm,
				HumanBodyBones.LeftUpperArm => DefaultLeftArm,
				HumanBodyBones.RightHand => DefaultRightArm,
				HumanBodyBones.RightUpperArm => DefaultRightArm,
				_ => DefaultRot
			};
		}
	}
}
