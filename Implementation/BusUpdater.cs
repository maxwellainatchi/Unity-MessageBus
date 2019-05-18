using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Messaging {

	[ExecuteInEditMode]
	public class BusUpdater: MonoBehaviour {
		static BusUpdater instance;

		public Dictionary<UpdateStage, Queue<Message.IMessage>> messageQueues = new Dictionary<UpdateStage, Queue<Message.IMessage>> {
			[UpdateStage.FixedUpdate] = new Queue<Message.IMessage>(),
			[UpdateStage.Update] = new Queue<Message.IMessage>(),
			[UpdateStage.LateUpdate] = new Queue<Message.IMessage>()
		};

		private void Awake() {
			if (BusUpdater.instance == null) {
				BusUpdater.instance = this;
			} else if (BusUpdater.instance != this) {
				if (Application.isPlaying) {
					Destroy(this);
				} else {
					DestroyImmediate(this);
				}
			}
		}

		private void FixedUpdate() {
			this.sendMessageForStage(UpdateStage.FixedUpdate);
		}

		// Update is called once per frame
		private void Update () {
			this.sendMessageForStage(UpdateStage.Update);
		}

		private void LateUpdate() {
			this.sendMessageForStage(UpdateStage.LateUpdate);
		}

		void sendMessageForStage(UpdateStage stage) {
			Queue<Message.IMessage> queue = this.messageQueues[stage];
			while (queue.Count > 0) {
				Messaging.Bus.main._sendMessageToHandlers(queue.Dequeue());
			}
		}

		public static void AddMessage(Message.IMessage msg) {
			BusUpdater.instance.messageQueues[msg.getUpdateStage()].Enqueue(msg);
		} 
	}
}
