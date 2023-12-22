using UnityEngine;

namespace OccaSoftware.Ninja.Runtime
{
	[AddComponentMenu("OccaSoftware/Performance/Ninja")]
	public class Ninja : MonoBehaviour
	{
		[SerializeField, Min(0f), Tooltip("Input your project's target frame rate.")]
		private float targetFrameRate = 60;

		[SerializeField, Min(0f), Tooltip("How long the widget should wait before starting to monitor the frame rate data. Can help prevent garbage data during Unity's play mode initialization.")]
		private float startDelay = 5f;

		[SerializeField, Min(0f), Tooltip("Controls the rate at which the Live section updates. Higher values mean the displayed values update less frequently, making it easier to read, but can hide frequent jitter.")]
		private float timeBetweeenDisplayUpdates = 0.2f;

		[SerializeField, Min(1f), Tooltip("Controls the amount of time it takes for the samples to converge for the average.")]
		private float timeToConverge = 10f;

		private GUISkin skin = null;


		private float factor;

		private PerformanceData frameTiming;
		private PerformanceData fps;
		private PerformanceData budget;

		private PerformanceData bracket1;
		private PerformanceData bracket2;
		private PerformanceData bracket3;

		private float previousUpdateTime;
		private Rect outerControlRect;

		private const float SECONDS_TO_MS = 1000.0f;
		private const float FRACTION_TO_PERCENT = 100.0f;

		void Start()
		{
			Setup();
		}

		void Setup()
		{
			fps = new PerformanceData(targetFrameRate);
			frameTiming = new PerformanceData(GetReciproal(fps.target));
			budget = new PerformanceData(1f);

			factor = frameTiming.target / timeToConverge;
			bracket1 = new PerformanceData(0.5f, 0.5f, 0.5f, fps.target * 0.50f);
			bracket2 = new PerformanceData(0.5f, 0.5f, 0.5f, fps.target * 0.75f);
			bracket3 = new PerformanceData(0.5f, 0.5f, 0.5f, fps.target * 1.00f);
		}

		private void Update()
		{
			if (startDelay > Time.unscaledTime)
				return;

			UpdateFrameTiming();
			UpdateFrameRate();
			UpdateBudget();
			UpdateBrackets();

			UpdateDisplayValues();
		}

		private void OnGUI()
		{
			SetupGUISkin();
			SetupGUI();

			DrawPrimary();
			DrawBrackets();
		}



		private void UpdateBrackets()
		{
			bracket1.live = fps.live > bracket1.target ? 1.0f : 0.0f;
			bracket2.live = fps.live > bracket2.target ? 1.0f : 0.0f;
			bracket3.live = fps.live > bracket3.target ? 1.0f : 0.0f;

			bracket1.average = Mathf.Lerp(bracket1.average, bracket1.live, factor);
			bracket2.average = Mathf.Lerp(bracket2.average, bracket2.live, factor);
			bracket3.average = Mathf.Lerp(bracket3.average, bracket3.live, factor);
		}

		private void UpdateDisplayValues()
		{
			if (Time.unscaledTime < previousUpdateTime + timeBetweeenDisplayUpdates)
				return;

			frameTiming.display = frameTiming.live;
			fps.display = fps.live;
			budget.display = budget.live;

			bracket1.display = bracket1.average;
			bracket2.display = bracket2.average;
			bracket3.display = bracket3.average;

			previousUpdateTime = Time.unscaledTime;
		}

		private void UpdateFrameTiming()
		{
			frameTiming.live = GetFrameTiming();
			frameTiming.average = Mathf.Lerp(frameTiming.average, frameTiming.live, factor);
		}

		private void UpdateFrameRate()
		{
			fps.live = GetReciproal(frameTiming.live);
			fps.average = GetReciproal(frameTiming.average);
		}

		private void UpdateBudget()
		{
			budget.live = frameTiming.live / frameTiming.target;
			budget.average = frameTiming.average / frameTiming.target;
		}

		private void DrawPrimary()
		{
			GUILayout.BeginArea(outerControlRect);

			GUILayout.BeginArea(new Rect(0, 0, 100, outerControlRect.height));
			GUILayout.Label("");
			DrawTextOutlined("FPS:", GetParagraphColor());
			DrawTextOutlined("MS:", GetParagraphColor());
			DrawTextOutlined("% of Budget:", GetParagraphColor());
			GUILayout.EndArea();


			GUILayout.BeginArea(new Rect(100, 0, 80, outerControlRect.height));
			DrawTextOutlined("Live", GetHeaderColor());
			DrawTextOutlined($"{fps.display:0.0}", GetConditionalColor(fps.display, fps.target));
			DrawTextOutlined($"{frameTiming.display * SECONDS_TO_MS:0.0}", GetConditionalColor(frameTiming.target, frameTiming.display));
			DrawTextOutlined($"{budget.display * FRACTION_TO_PERCENT:0.0}", GetConditionalColor(budget.target, budget.display));
			GUILayout.EndArea();


			GUILayout.BeginArea(new Rect(180, 0, 80, outerControlRect.height));
			DrawTextOutlined("Avg", GetHeaderColor());
			DrawTextOutlined($"{fps.average:0.0}", GetConditionalColor(fps.average, fps.target));
			DrawTextOutlined($"{frameTiming.average * SECONDS_TO_MS:0.0}", GetConditionalColor(frameTiming.target, frameTiming.average));
			DrawTextOutlined($"{budget.average * FRACTION_TO_PERCENT:0.0}", GetConditionalColor(budget.target, budget.average));
			GetHeaderColor();
			GUILayout.EndArea();

			GUILayout.EndArea();
		}

		private void DrawBrackets()
		{
			GUILayout.BeginArea(new Rect(outerControlRect.x, outerControlRect.y + outerControlRect.height, outerControlRect.width, outerControlRect.height));

			GUILayout.BeginArea(new Rect(0, 0, 100, outerControlRect.height));
			GetParagraphColor();
			GUILayout.Label("");
			DrawTextOutlined("% Above:", GetParagraphColor());
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(110, 0, 50, outerControlRect.height));
			DrawTextOutlined($"{bracket1.target:0}", GetHeaderColor());
			DrawTextOutlined($"{(bracket1.display * FRACTION_TO_PERCENT):0.0}", GetParagraphColor());
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(160, 0, 50, outerControlRect.height));
			DrawTextOutlined($"{bracket2.target:0}", GetHeaderColor());
			DrawTextOutlined($"{(bracket2.display * FRACTION_TO_PERCENT):0.0}", GetParagraphColor());
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(210, 0, 50, outerControlRect.height));
			DrawTextOutlined($"{bracket3.target:0}", GetHeaderColor());
			DrawTextOutlined($"{(bracket3.display * FRACTION_TO_PERCENT):0.0}", GetParagraphColor());
			GUILayout.EndArea();

			GUILayout.EndArea();
		}

		private void DrawTextOutlined(string text, Color color)
		{
			Rect rect = GUILayoutUtility.GetRect(new GUIContent(text), skin.label);
			rect.y += 1;
			rect.x += 1;
			skin.label.normal.textColor = Color.black;
			skin.label.hover = skin.label.normal;
			GUI.Label(rect, text);
			rect.y -= 1;
			rect.x -= 1;
			skin.label.normal.textColor = color;
			skin.label.hover = skin.label.normal;
			GUI.Label(rect, text);
		}

		private Color GetConditionalColor(float a, float b)
		{
			if (a < b)
			{
				return Color.red;
			}
			return Color.green;
		}

		private Color GetParagraphColor()
		{
			return new Color(0.8f, 0.8f, 0.8f);
		}

		private Color GetHeaderColor()
		{
			return Color.white;
		}

		private void SetupGUISkin()
		{
			if (skin == null)
			{
				skin = Instantiate(GUI.skin);
			}
			skin.label.fontStyle = FontStyle.Bold;
			skin.label.alignment = TextAnchor.MiddleRight;
			skin.label.fontSize = 16;
		}

		private void SetupGUI()
		{
			outerControlRect = GetControlRect();
			GUI.skin = skin;
		}

		private float GetFrameTiming()
		{
			return Time.unscaledDeltaTime;
		}

		private float GetReciproal(float a)
		{
			return 1.0f / a;
		}

		private Rect GetControlRect()
		{
			return new Rect(Screen.width - 350, 50, 300, 120);
		}


		private struct PerformanceData
		{
			public float live;
			public float average;
			public float display;
			public float target;

			public PerformanceData(float live, float average, float display, float target)
			{
				this.live = live;
				this.average = average;
				this.display = display;
				this.target = target;
			}

			public PerformanceData(float target)
			{
				live = target;
				average = target;
				display = target;
				this.target = target;
			}
		}
	}
}