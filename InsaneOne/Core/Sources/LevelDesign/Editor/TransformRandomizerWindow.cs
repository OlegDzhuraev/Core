using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace InsaneOne.Core.LevelDesign
{
	public class TransformRandomizerWindow : EditorWindow
	{
		const string StylesPath = "InsaneOne/ToolsStyles";
		const string GroupStyleName = "group-box";

		[Flags]
		public enum Axes
		{
			None = 0,
			X = 1,
			Y = 2,
			Z = 4
		}

		EnumFlagsField rotationAxesField;
		Slider maxRotationAngleField;

		EnumFlagsField sizeAxesField;
		Label sizeSliderValueLabel;
		MinMaxSlider sizeSlider;

		EnumFlagsField positionAxesField;
		Slider positionTiltShiftSlider;

		[MenuItem("Tools/InsaneOne/Level Design/Transform Randomizer")]
		static void Init()
		{
			var window = (TransformRandomizerWindow)GetWindow(typeof(TransformRandomizerWindow), false, "Transform Randomizer", true);
			window.Show();
		}

		void CreateGUI()
		{
			var defaultAxes = Axes.X | Axes.Y | Axes.Z;
			var style = Resources.Load(StylesPath) as StyleSheet;
			var root = rootVisualElement;
			root.styleSheets.Add(style);

			var infoLabel = new Label("This tool randomizes the parameters of transforms selected in the scene")
			{
				style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) }
			};

			var infoBox = new VisualElement();
			infoBox.AddToClassList(GroupStyleName);
			infoBox.Add(infoLabel);

			var rotationBox = new VisualElement();
			rotationBox.AddToClassList(GroupStyleName);

			rotationAxesField = new EnumFlagsField("Rotation Axes", defaultAxes);
			rotationAxesField.RegisterValueChangedCallback(OnRotationAxisChanged);
			maxRotationAngleField = new Slider("Rotation randomize angle", 0f, 180f) { showInputField = true };

			rotationBox.Add(rotationAxesField);
			rotationBox.Add(maxRotationAngleField);

			var sizeBox = new Box();
			sizeBox.AddToClassList(GroupStyleName);

			sizeAxesField = new EnumFlagsField("Size Axes",defaultAxes);
			sizeAxesField.RegisterValueChangedCallback(OnSizeAxesChanged);
			sizeSliderValueLabel = new Label("Min: 0.5 | Max: 1.5")
			{
				style = { unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter) },
			};
			sizeSlider = new MinMaxSlider("Scale random min max value", 0.5f, 1.5f, 0f, 10f);
			sizeSlider.RegisterValueChangedCallback(OnSizeSliderChanged);

			sizeBox.Add(sizeAxesField);
			sizeBox.Add(sizeSliderValueLabel);
			sizeBox.Add(sizeSlider);

			var posBox = new Box();
			posBox.AddToClassList(GroupStyleName);

			positionAxesField = new EnumFlagsField("Position Axes", defaultAxes);
			positionAxesField.RegisterValueChangedCallback(OnPositionAxesChanged);
			positionTiltShiftSlider = new Slider("Position tilt shift max value") { showInputField = true };

			posBox.Add(positionAxesField);
			posBox.Add(positionTiltShiftSlider);

			var randomizeBtn = new Button(OnButtonClicked)
			{
				text = "Randomize",
				style = { width = 150, height = 50, alignSelf = new StyleEnum<Align>(Align.Center) },
			};

			root.Add(infoBox);
			root.Add(rotationBox);
			root.Add(sizeBox);
			root.Add(posBox);
			root.Add(randomizeBtn);
		}

		void OnSizeAxesChanged(ChangeEvent<Enum> ev)
		{
			var display = GetDisplay((Axes)ev.newValue != Axes.None);
			sizeSliderValueLabel.style.display = display;
			sizeSlider.style.display = display;
		}

		void OnSizeSliderChanged(ChangeEvent<Vector2> ev)
		{
			var x = Math.Round(ev.newValue.x, 2);
			var y = Math.Round(ev.newValue.y, 2);
			sizeSliderValueLabel.text = $"Min: {x} | Max: {y}";
		}

		void OnRotationAxisChanged(ChangeEvent<Enum> ev) => maxRotationAngleField.style.display = GetDisplay((Axes)ev.newValue != Axes.None);
		void OnPositionAxesChanged(ChangeEvent<Enum> ev) => positionTiltShiftSlider.style.display = GetDisplay((Axes)ev.newValue != Axes.None);
		static StyleEnum<DisplayStyle> GetDisplay(bool isEnabled) => new (isEnabled ? StyleKeyword.Auto : StyleKeyword.None);

		void OnButtonClicked()
		{
			foreach (var obj in Selection.objects)
				if (obj is GameObject go) // todo check it on scene
					RandomizeNew(go);
		}

		void RandomizeNew(GameObject go)
		{
			var tr = go.transform;

			var rotationAxes = (Axes)rotationAxesField.value;
			var sizeAxes = (Axes)sizeAxesField.value;
			var positionAxes = (Axes)positionAxesField.value;
			var positionMaxTilt = positionTiltShiftSlider.value;
			var maxRotationAngle = maxRotationAngleField.value;

			if (rotationAxes != Axes.None)
				tr.Rotate(GetAxisValuedVector3(rotationAxes) * Random.Range(-maxRotationAngle, maxRotationAngle));

			if (sizeAxes != Axes.None)
			{
				var finalScale = tr.localScale;
				var scaleChange = GetAxisValuedVector3(sizeAxes) * Random.Range(sizeSlider.value.x, sizeSlider.value.y);

				ApplyNonZeroTo(ref finalScale.x, scaleChange.x);
				ApplyNonZeroTo(ref finalScale.y, scaleChange.y);
				ApplyNonZeroTo(ref finalScale.z, scaleChange.z);

				tr.localScale = finalScale;
			}

			if (positionAxes != Axes.None)
				tr.position += GetAxisValuedVector3(positionAxes) * Random.Range(-positionMaxTilt, positionMaxTilt);
		}

		static void ApplyNonZeroTo(ref float target, float value)
		{
			if (!Mathf.Approximately(value, 0))
				target = value;
		}

		static Vector3 GetAxisValuedVector3(Axes axes)
		{
			return new Vector3((axes & Axes.X) != 0 ? 1 : 0, (axes & Axes.Y) != 0 ? 1 : 0, (axes & Axes.Z) != 0 ? 1 : 0);
		}
	}
}