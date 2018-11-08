using UnityEngine;

public interface iFloatingTextPositioner
{
	bool GetPosition(ref Vector2 position, GUIContent content, Vector2 size);
}
