//======= Copyright (c) Valve Corporation, All rights reserved. ===============
//
// Purpose: Allows Enums to be shown in the inspector as flags
//
//=============================================================================

using UnityEngine;
#if false
using UnityEditor;
#endif

namespace Valve.VR.InteractionSystem
{
	//-------------------------------------------------------------------------
	public class EnumFlags : PropertyAttribute
	{
		public EnumFlags() { }
	}


#if false
	//-------------------------------------------------------------------------
	[CustomPropertyDrawer( typeof( EnumFlags ) )]
	public class EnumFlagsPropertyDrawer : PropertyDrawer
	{
		public override void OnGUI( Rect position, SerializedProperty property, GUIContent label )
		{
			property.intValue = EditorGUI.MaskField( position, label, property.intValue, property.enumNames );
		}
	}
#endif
}
