using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour {
	// This class is the message that will be emitted. It also includes a default handler, which isn't required.
	public class ShouldChange : Messaging.Message.IMessage {
		// The color to change to.
		public Color color;
		public bool isFinal;

		// When the message is created, it needs to know what color to change to.
		public ShouldChange(Color c) {
			this.color = c;
		}

		// The default handler just changes the material color.
		public class SetColorHandler : Messaging.Handler<ShouldChange> {
			// It requires a renderer to change the color.
			public Renderer renderer;
			// IMPORTANT: overriding the MessageHandler<T> constructor, you need to call the superconstructor!
			// Otherwise it won't register itself on the main MessageBus.
			public SetColorHandler(Renderer renderer) : base() {
				this.renderer = renderer;
			}

			// When the message is handled, change the color to the message color.
			public override void handleMessage(ShouldChange msg) {
				this.renderer.material.color = msg.color;
			}
		}

		public class SetIndexHandler : Messaging.Handler<ShouldChange> {
			public Dropdown dropdown;

			public SetIndexHandler(Dropdown dropdown) : base() {
				this.dropdown = dropdown;
			}

			// When the color changes, so should the dropdown
			public override void handleMessage(ShouldChange msg) {
				// Note: this will be triggered even when the dropdown is what caused the color change. This is intentional behavior;
				// it's supposed to be reactive.
				this.dropdown.SetValueWithoutNotify(ChangeColor.indexForColor(msg.color));
			}
		}
	}

	// The event listener. Emits a change color message when the dropdown changes.
	public void ShouldChangeColor(int index) {
		// Get the color represented by the dropdown.
		Color color = ChangeColor.colorForIndex(index);

		// Emit a new message with the given color.
		new ShouldChange(color).emitSelf();
	}

	public void ShouldChangeColor1000Times() {
		for (int i = 0; i < 1000; ++i) {
			// Get the color represented by the dropdown.
			Color color = ChangeColor.colorForIndex(Random.Range(0, 3));
			// Emit a new message with the given color.
			new ShouldChange(color).emitSelf();
		}
	}

	private Messaging.Handler<ShouldChange> dropdownHandler;
	private void Awake() {
		Dropdown dropdown = this.GetComponent<Dropdown>();
		if (dropdown != null) {
			this.dropdownHandler = new ShouldChange.SetIndexHandler(dropdown);
		}
	}

	// Figures out which color is represented, given its index in the dropdown.
	public static Color colorForIndex(int index) {
		if (index == 0) return Color.red;
		if (index == 1) return Color.green;
		/*if (index == 2)*/return Color.blue;
	}

	public static int indexForColor(Color color) {
		 if (color == Color.red) return 0;
		 if (color == Color.green) return 1;
		 /*if (color == Color.blue)*/ return 2;
	}
}
