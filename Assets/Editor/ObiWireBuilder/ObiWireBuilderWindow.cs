using System;
using System.IO;
using Obi;
using UnityEditor;
using UnityEngine;

namespace Project.EditorTools
{
    public sealed class ObiWireBuilderWindow : EditorWindow
    {
        [SerializeField] private ObiSolver solver;
        [SerializeField] private Transform startPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private float sag = 0.25f;
        [SerializeField] private Vector3 sagDirection = Vector3.down;
        [SerializeField] private int controlPointCount = 8;
        [SerializeField] private float ropeThickness = 0.015f;
        [SerializeField] private float resolution = 1f;
        [SerializeField] private float particleMass = 0.1f;
        [SerializeField] private ObiRopeSection ropeSection;
        [SerializeField] private string wireName = "ObiWire";

        [MenuItem("Tools/Obi/Wire Builder")]
        private static void Open()
        {
            GetWindow<ObiWireBuilderWindow>("Obi Wire Builder");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Endpoints", EditorStyles.boldLabel);
            solver = (ObiSolver)EditorGUILayout.ObjectField("Solver", solver, typeof(ObiSolver), true);
            startPoint = (Transform)EditorGUILayout.ObjectField("Start Point", startPoint, typeof(Transform), true);
            endPoint = (Transform)EditorGUILayout.ObjectField("End Point", endPoint, typeof(Transform), true);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Shape", EditorStyles.boldLabel);
            sag = EditorGUILayout.Slider("Sag", sag, 0, 2f);
            sagDirection = EditorGUILayout.Vector3Field("Sag Direction", sagDirection);
            controlPointCount = EditorGUILayout.IntSlider("Curve Samples", controlPointCount, 2, 24);

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);
            ropeThickness = EditorGUILayout.FloatField("Thickness", ropeThickness);
            resolution = EditorGUILayout.Slider("Resolution", resolution, 0.1f, 2f);
            particleMass = EditorGUILayout.FloatField("Particle Mass", particleMass);
            ropeSection = (ObiRopeSection)EditorGUILayout.ObjectField(
                "Extruded Section (optional)", ropeSection, typeof(ObiRopeSection), false);

            EditorGUILayout.Space(8);
            wireName = EditorGUILayout.TextField("Wire Name", wireName);

            using (new EditorGUI.DisabledScope(!CanGenerate()))
            {
                if (GUILayout.Button("Generate Wire", GUILayout.Height(34)))
                    GenerateWire();
            }

            EditorGUILayout.HelpBox(
                "Создаёт ObiRopeBlueprint, объект ObiRope и два Static ObiParticleAttachment. " +
                "Концы кривой изначально совпадают с заданными Transform.",
                MessageType.Info);
        }

        private bool CanGenerate()
        {
            return solver != null && startPoint != null && endPoint != null &&
                   startPoint != endPoint && ropeThickness > 0 &&
                   resolution > 0 && particleMass > 0 &&
                   controlPointCount >= 2 &&
                   (endPoint.position - startPoint.position).sqrMagnitude > 0.000001f;
        }

        private void GenerateWire()
        {
            string folder = "Assets/ObiWires";
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets", "ObiWires");

            string assetPath = AssetDatabase.GenerateUniqueAssetPath(
                folder + "/" + SanitizeFileName(wireName) + "_Blueprint.asset");

            GameObject ropeObject = null;
            ObiRopeBlueprint blueprint = null;

            try
            {
                ropeObject = new GameObject(string.IsNullOrWhiteSpace(wireName) ? "ObiWire" : wireName);
                Undo.RegisterCreatedObjectUndo(ropeObject, "Create Obi Wire");
                ropeObject.transform.SetParent(solver.transform, false);
                ropeObject.transform.localPosition = Vector3.zero;
                ropeObject.transform.localRotation = Quaternion.identity;
                ropeObject.transform.localScale = Vector3.one;

                blueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
                blueprint.name = Path.GetFileNameWithoutExtension(assetPath);
                blueprint.resolution = resolution;
                blueprint.thickness = ropeThickness;
                blueprint.path.Clear();

                BuildPath(blueprint, ropeObject.transform);
                blueprint.path.FlushEvents();
                blueprint.GenerateImmediate();

                AssetDatabase.CreateAsset(blueprint, assetPath);
                AssetDatabase.SaveAssets();

                var rope = ropeObject.AddComponent<ObiRope>();
                rope.ropeBlueprint = blueprint;

                var startAttachment = ropeObject.AddComponent<ObiParticleAttachment>();
                startAttachment.target = startPoint;
                startAttachment.particleGroup = blueprint.groups[0];
                startAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;

                var endAttachment = ropeObject.AddComponent<ObiParticleAttachment>();
                endAttachment.target = endPoint;
                endAttachment.particleGroup = blueprint.groups[blueprint.groups.Count - 1];
                endAttachment.attachmentType = ObiParticleAttachment.AttachmentType.Static;

                if (ropeSection != null)
                {
                    var renderer = ropeObject.AddComponent<ObiRopeExtrudedRenderer>();
                    renderer.section = ropeSection;
                }

                EditorUtility.SetDirty(ropeObject);
                Selection.activeGameObject = ropeObject;
                SceneView.lastActiveSceneView?.FrameSelected();

                Debug.Log($"Obi Wire создан. Blueprint: {assetPath}", ropeObject);
            }
            catch (Exception exception)
            {
                if (ropeObject != null)
                    Undo.DestroyObjectImmediate(ropeObject);

                if (blueprint != null && !AssetDatabase.Contains(blueprint))
                    DestroyImmediate(blueprint);

                Debug.LogException(exception);
                EditorUtility.DisplayDialog("Obi Wire Builder", 
                    "Не удалось создать провод. Подробности находятся в Console.\n\n" + exception.Message,
                    "OK");
            }
        }

        private void BuildPath(ObiRopeBlueprint blueprint, Transform ropeTransform)
        {
            Vector3 start = ropeTransform.InverseTransformPoint(startPoint.position);
            Vector3 end = ropeTransform.InverseTransformPoint(endPoint.position);

            Vector3 sagWorld = sagDirection.sqrMagnitude > 0.0001f
                ? sagDirection.normalized * sag
                : Vector3.down * sag;
            Vector3 sagLocal = ropeTransform.InverseTransformVector(sagWorld);

            // Quadratic Bezier: the midpoint is displaced by exactly 'sag'.
            Vector3 control = (start + end) * 0.5f + sagLocal * 2f;
            int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);

            for (int i = 0; i < controlPointCount; ++i)
            {
                float t = i / (float)(controlPointCount - 1);
                float oneMinusT = 1f - t;

                Vector3 position =
                    oneMinusT * oneMinusT * start +
                    2f * oneMinusT * t * control +
                    t * t * end;

                Vector3 derivative =
                    2f * oneMinusT * (control - start) +
                    2f * t * (end - control);

                // Approximate Bezier handles for a smooth path through sampled points.
                Vector3 tangent = derivative / (controlPointCount - 1) * 0.5f;

                string pointName = i == 0 ? "Start" :
                    i == controlPointCount - 1 ? "End" : $"Point_{i:00}";

                blueprint.path.AddControlPoint(
                    position,
                    -tangent,
                    tangent,
                    Vector3.up,
                    particleMass,
                    particleMass,
                    ropeThickness,
                    filter,
                    Color.white,
                    pointName);
            }
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "ObiWire";

            foreach (char invalid in Path.GetInvalidFileNameChars())
                value = value.Replace(invalid, '_');

            return value.Trim();
        }

        private void OnDrawGizmos()
        {
            if (startPoint == null || endPoint == null)
                return;

            Handles.color = Color.green;
            Handles.SphereHandleCap(0, startPoint.position, Quaternion.identity, 0.025f, EventType.Repaint);
            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, endPoint.position, Quaternion.identity, 0.025f, EventType.Repaint);

            Vector3 direction = sagDirection.sqrMagnitude > 0.0001f
                ? sagDirection.normalized
                : Vector3.down;

            Vector3 midpoint = (startPoint.position + endPoint.position) * 0.5f;
            Vector3 control = midpoint + direction * sag * 2f;

            Handles.color = Color.yellow;
            Vector3 previous = startPoint.position;
            const int segments = 32;
            for (int i = 1; i <= segments; ++i)
            {
                float t = i / (float)segments;
                float u = 1f - t;
                Vector3 current = u * u * startPoint.position +
                                  2f * u * t * control +
                                  t * t * endPoint.position;
                Handles.DrawLine(previous, current);
                previous = current;
            }
        }
    }
}