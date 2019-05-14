using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeColor : MonoBehaviour
{
	// This class is the message that will be emitted. It also includes a default handler, which isn't required.
    public class ShouldChange: Message.IMessage 
    {
		// The color to change to.
		public Color color;

		// When the message is created, it needs to know what color to change to.
		public ShouldChange(Color c)
		{
			this.color = c;
		}

		// The default handler just changes the material color.
        public class Handler: MessageHandler<ShouldChange>
        {
			// It requires a renderer to change the color.
            public Renderer renderer;
			// IMPORTANT: overriding the MessageHandler<T> constructor, you need to call the superconstructor!
			// Otherwise it won't register itself on the main MessageBus.
            public Handler(Renderer renderer): base() 
            {
                this.renderer = renderer;
            }

			// When the message is handled, change the color to the message color.
            public override void handleTypedMessage(ShouldChange msg)
            {
                this.renderer.material.color = msg.color;
            }
        }
    }
	
	// The color dropdown that this is attached to.
	Dropdown dropdown;	
	void Awake()
	{
		this.dropdown = this.GetComponent<Dropdown>();
		// Make sure a message is emitted when the dropdown changes.
		this.dropdown.onValueChanged.AddListener(this.ShouldChangeColor);
	}
	
	// The event listener. Emits a change color message when the dropdown changes.
	public void ShouldChangeColor(int index)
	{
		// Get the color represented by the dropdown.
		Color color = this.colorForSelection(index);
		// Emit a new message with the given color.
        MessageBus.main.emit(new ShouldChange(color));
    }

	// Figures out which color is represented, given its index in the dropdown.
	public Color colorForSelection(int index) {
		if (index == 0) return Color.red;
		if (index == 1) return Color.green;
		/*if (index == 3)*/return Color.blue;
	}
}