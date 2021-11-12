using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityTools.Attributes;
using UnityTools.Debuging;

namespace UnityTools.Physics
{
	[System.Serializable]
	public class AccumulatorTimestep
	{
		public enum State
		{
			DeltaTime,//use delta time for update
			PreferredTime,// use preferredTimestep
			PreferredTimeOverTime,// use preferredTimestep but can not catch real world time
			
		}
		protected Action<float> updateActions;
		[SerializeField] protected float accumulator = 0f;
		[SerializeField] protected float preferredTimestep = 1 / 60f;
		[SerializeField] protected int maxIteration = 32;
		[SerializeField] protected const float MaxAccumulationTime = 1f;
		[SerializeField] protected bool logWarning = false;
		[SerializeField] protected bool alwaysPreferredTimestep = false;
		[SerializeField, DisableEdit] protected State currentState = State.DeltaTime;
		public AccumulatorTimestep(float preferredTimestep = 1 / 60f, int maxIteration = 32, bool alwaysPreferredTimestep = false, bool logWarning = false)
        {
            this.preferredTimestep = preferredTimestep;
            this.maxIteration = maxIteration;
            this.accumulator = 0;
			this.alwaysPreferredTimestep = alwaysPreferredTimestep;

			this.logWarning = logWarning;
        }
		public void Update(float preferredTimestep, int maxIteration)
		{
            this.preferredTimestep = preferredTimestep;
            this.maxIteration = maxIteration;
            this.Update();
        }
		public void Update()
		{
			this.accumulator += Time.deltaTime;
            var iteration = 0;
			while (this.accumulator >= this.preferredTimestep && iteration < this.maxIteration)
			{
				var dt = this.preferredTimestep;
				this.updateActions?.Invoke(dt);
				this.accumulator -= dt;
				iteration++;

				this.currentState = State.PreferredTime;
			}
			if(iteration == 0) 
			{
				var dt = this.alwaysPreferredTimestep ? this.preferredTimestep : Time.deltaTime;
				this.updateActions?.Invoke(dt);
				this.accumulator -= dt;
				this.accumulator = math.clamp(this.accumulator, 0, MaxAccumulationTime);

				this.currentState = State.DeltaTime;
			}

            if(iteration >= this.maxIteration)
            {
				if (this.logWarning) LogTool.Log("Max Iteration reached " + iteration, LogLevel.Warning);
                if(this.accumulator > MaxAccumulationTime)
				{
					if (this.logWarning) LogTool.Log("Reset accumulator", LogLevel.Warning);
					this.accumulator = 0;
				}
				this.currentState = State.PreferredTimeOverTime;
            }
		}
		public void OnUpdate(Action<float> update)
		{
			this.updateActions -= update;
			this.updateActions += update;
		}
	}
}
