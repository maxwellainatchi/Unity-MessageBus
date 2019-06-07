using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public class SwitchDirection : MonoBehaviour {
	// This class is the message that will be emitted. It only includes a default handler, which isn't required. This class could be empty, as long as it
	// extends Messaging.IMessage.
	public class ShouldChange : Messaging.Message.IMessage {
		// The default handler. Reverses the angular velocity.
		public class Handler : Messaging.Handler<ShouldChange> {
			// The RigidBody required to get/set the angular velocity.
			public Rigidbody rb;
			public Handler(Rigidbody rb) {
				this.rb = rb;
			}

			// When receiving the message, swap the angular velocity.
			public override void handleMessage(ShouldChange msg) {
				rb.angularVelocity = -rb.angularVelocity;
			}
		}
	}

	void Awake() {
		// send a message on button click.
		this.GetComponent<Button>().onClick.AddListener(this.DidClickButton);
	}

	public void DidClickButton() {
		// emit a new change message when the button is clicked.
		new ShouldChange().emitSelf();
	}
}
