using UnityEngine;

public enum TransformType
{
    position,
    rotation,
    both
}
public class TransformObject : MonoBehaviour
{
    // a script to move an object to another object
    
    public Transform thisObject;
    public Transform target;
    public TransformType TransformType;
    public bool lerped;
    public float lerpTime = 1f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        if (lerped)
        {
            if (TransformType == TransformType.position)
            {
                thisObject.position = Vector3.Lerp(thisObject.position, target.position, lerpTime);
            }
            else if (TransformType == TransformType.rotation)
            {
                thisObject.rotation = Quaternion.Lerp(thisObject.rotation, target.rotation, lerpTime);
            }
            else if (TransformType == TransformType.both)
            {
                thisObject.position = Vector3.Lerp(thisObject.position, target.position, lerpTime);
                thisObject.rotation = Quaternion.Lerp(thisObject.rotation, target.rotation, lerpTime);
            }
        }
        else
        {
            if (TransformType == TransformType.position)
            {
                thisObject.position = target.position;
            }
            else if (TransformType == TransformType.rotation)
            {
                thisObject.rotation = target.rotation;
            }
            else if (TransformType == TransformType.both)
            {
                thisObject.position = target.position;
                thisObject.rotation = target.rotation;
            }
        }
    }
}
