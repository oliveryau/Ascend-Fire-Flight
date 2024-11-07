using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] public Transform swayTransform;

    [Header("Sway Properties")]
    [SerializeField] private float swayAmount;
    public float maxSwayAmount;
    public float swaySmooth;
    public AnimationCurve swayCurve;

    [Range(0f, 1f)] public float swaySmoothCounteraction;

    [Header("Rotation")]
    public float rotationSwayMultiplier;

    [Header("Position")]
    public float positionSwayMultiplier;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector2 sway;
    Quaternion lastRot;

    private void Reset()
    {
        Keyframe[] ks = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 0, 0) };
        swayCurve = new AnimationCurve(ks);
    }

    private void Start()
    {
        if (!swayTransform) swayTransform = transform;
        lastRot = transform.localRotation;
        initialPosition = swayTransform.localPosition;
        initialRotation = swayTransform.localRotation;
    }

    private void Update()
    {
        var angularVelocity = Quaternion.Inverse(lastRot) * transform.rotation;

        float mouseX = FixAngle(angularVelocity.eulerAngles.y) * swayAmount;
        float mouseY = -FixAngle(angularVelocity.eulerAngles.x) * swayAmount;

        lastRot = transform.rotation;

        sway = Vector2.MoveTowards(sway, Vector2.zero, swayCurve.Evaluate(Time.deltaTime * swaySmoothCounteraction * sway.magnitude * swaySmooth));
        sway = Vector2.ClampMagnitude(new Vector2(mouseX, mouseY) + sway, maxSwayAmount);

        swayTransform.localPosition = Vector3.Lerp(swayTransform.localPosition, new Vector3(sway.x, sway.y, 0) * positionSwayMultiplier * Mathf.Deg2Rad + initialPosition, swayCurve.Evaluate(Time.deltaTime * swaySmooth));
        swayTransform.localRotation = Quaternion.Slerp(swayTransform.localRotation, initialRotation * Quaternion.Euler(Mathf.Rad2Deg * rotationSwayMultiplier * new Vector3(-sway.y, sway.x, 0)), swayCurve.Evaluate(Time.deltaTime * swaySmooth));
    }

    private float FixAngle(float angle)
    {
        return Mathf.Repeat(angle + 180f, 360f) - 180f;
    }
}
