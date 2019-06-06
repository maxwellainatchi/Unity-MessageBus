using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Messaging {
	public enum UpdateStage {
		Immediate,
		Update,
		LateUpdate,
		FixedUpdate,
	}

	[ExecuteInEditMode]
	public class BusUpdater : MonoBehaviour {
		public static BusUpdater instance { get; private set; }

		public UpdateStageUIntSerializableDictionary timeLimits = new UpdateStageUIntSerializableDictionary();
		public UpdateStageUIntSerializableDictionary countLimits = new UpdateStageUIntSerializableDictionary();

		private readonly Dictionary<UpdateStage, Queue<Message.IMessage>> messageQueues = new Dictionary<UpdateStage, Queue<Message.IMessage>> {
			[UpdateStage.FixedUpdate] = new Queue<Message.IMessage>(),
			[UpdateStage.Update] = new Queue<Message.IMessage>(),
			[UpdateStage.LateUpdate] = new Queue<Message.IMessage>()
		};

		// MARK: Monobehaviour

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
		private void Update() {
			this.sendMessageForStage(UpdateStage.Update);
		}

		private void LateUpdate() {
			this.sendMessageForStage(UpdateStage.LateUpdate);
		}

		// MARK: Internal methods

		private void sendMessageForStage(UpdateStage stage) {
			System.DateTime startTime = System.DateTime.Now;
			Queue<Message.IMessage> queue = this.messageQueues[stage];
			int count = 0;
			System.Func<bool> isWithinLimits = () => {
				float? timeLimit = this.timeLimits.TryGetValue(stage);
				uint? countLimit = this.countLimits.TryGetValue(stage);
				if (timeLimit.HasValue && (System.DateTime.Now - startTime).TotalMilliseconds > timeLimit.Value) return false;
				if (countLimit.HasValue && count >= countLimit.Value) return false;
				return queue.Count > 0;
			};
			while (isWithinLimits()) {
				Messaging.Bus.main._sendMessageToHandlers(queue.Dequeue());
				count++;
			}
		}

		public static void AddMessage(Message.IMessage msg) {
			BusUpdater.instance.messageQueues[msg.getUpdateStage()].Enqueue(msg);
		}
	}
}
