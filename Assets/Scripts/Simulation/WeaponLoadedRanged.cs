using System;

namespace Simulation
{
	public class WeaponLoadedRanged : Weapon
	{
		public override void Init(Hero by, World world)
		{
			base.Init(by, world);
			this.isActive = false;
			this.attackTime = 0f;
			this.waitTime = 0f;
			this.hasThrown = false;
			this.isReloading = true;
			this.load = 0;
		}

		public override Weapon Clone()
		{
			return new WeaponLoadedRanged
			{
				durAttack = this.durAttack,
				durWait = this.durWait,
				damageType = this.damageType,
				damageMoment = this.damageMoment,
				loadMax = this.loadMax,
				durReload = this.durReload,
				soundReload = this.soundReload,
				projectileType = this.projectileType,
				targetType = this.targetType,
				durFly = this.durFly,
				projectilePath = this.projectilePath,
				projectileImpactVis = this.projectileImpactVis,
				soundsAttack = this.soundsAttack,
				id = this.id,
				projectileIndexPattern = this.projectileIndexPattern
			};
		}

		public void SetTiming(float durAttack, float damageMoment, float durAdditionalWait)
		{
			this.durAttack = durAttack;
			this.durWait = durAttack + durAdditionalWait;
			this.damageMoment = damageMoment;
		}

		public override double GetDps()
		{
			return this.by.GetDamage() / (double)this.durWait;
		}

		public override float GetBarTimeRatio()
		{
			if (this.isReloading)
			{
				return this.reloadTime / this.durReload;
			}
			return GameMath.GetMinFloat(1f, (float)this.load / (float)this.GetLoadMax());
		}

		public override float GetAnimTimeRatio()
		{
			if (this.isReloading)
			{
				return this.reloadTime / this.durReload;
			}
			return this.attackTime / this.durAttack;
		}

		public override void UpdateActive(float dt)
		{
			this.UpdateWaitTime(dt);
			this.UpdateReloading(dt);
			if (this.isReloading)
			{
				return;
			}
			float num = this.by.GetAttackSpeed();
			if (num < 0.5f)
			{
				num = 0.5f;
			}
			else if (num > 1f)
			{
			}
			this.attackTime += dt * num;
			if (this.attackTime > this.damageMoment && !this.hasThrown)
			{
				this.Throw();
				this.load--;
			}
			if (this.attackTime > this.durAttack)
			{
				this.attackTime = 0f;
				this.hasThrown = false;
				this.isActive = false;
				this.numHits++;
			}
			base.PlayTimedSounds(this.attackTime / this.durAttack);
		}

		public override void UpdatePassive(float dt)
		{
			this.UpdateWaitTime(dt);
		}

		private void UpdateWaitTime(float dt)
		{
			float num = this.by.GetAttackSpeed();
			if (num < 0f)
			{
				num = 0f;
			}
			this.waitTime += dt * num;
		}

		private void UpdateReloading(float dt)
		{
			if (this.isReloading)
			{
				if (this.reloadTime == 0f)
				{
					if (this.soundReload is SoundVaried)
					{
						SoundVaried soundVaried = (SoundVaried)this.soundReload;
						soundVaried.SetVariation(this.numHits / this.GetLoadMax());
					}
					SoundEventSound e = new SoundEventSound(SoundType.GAMEPLAY, this.by.GetId(), false, 0f, this.soundReload);
					this.world.AddSoundEvent(e);
				}
				this.reloadTime += dt * this.by.GetReloadSpeed();
				if (this.reloadTime >= this.durReload)
				{
					this.isReloading = false;
					this.reloadTime = 0f;
					this.load = this.GetLoadMax();
				}
			}
		}

		private int GetLoadMax()
		{
			return this.loadMax + this.by.GetWeaponLoadExtra();
		}

		private void Throw()
		{
			this.hasThrown = true;
			UnitHealthy unitHealthy = this.target;
			if (this.immediatelyTarget != null)
			{
				this.target = this.immediatelyTarget;
				this.immediatelyTarget = null;
			}
			if (this.target == null || !this.target.IsAlive() || !this.target.IsOnWorld())
			{
				this.target = this.world.GetRandomAliveEnemy();
			}
			if (this.target != unitHealthy)
			{
				base.OnAttackTargetChanged(unitHealthy, this.target);
			}
			bool flag = false;
			float missChance = this.by.GetMissChance();
			if (GameMath.GetProbabilityOutcome(missChance, GameMath.RandType.NoSeed))
			{
				flag = true;
				Damage damage = new Damage(0.0, false, false, true, false);
				GlobalPastDamage pastDamage = new GlobalPastDamage(this.by, damage);
				this.world.AddPastDamage(pastDamage);
			}
			double num = this.by.GetDamage();
			float critChance = this.by.GetCritChance();
			bool probabilityOutcome = GameMath.GetProbabilityOutcome(critChance, GameMath.RandType.NoSeed);
			if (probabilityOutcome)
			{
				num *= this.by.GetCritFactor();
			}
			Damage damage2 = new Damage(num, probabilityOutcome, false, flag, false);
			damage2.type = this.damageType;
			if (flag)
			{
				this.by.OnMissed(this.target, damage2);
			}
			Projectile projectile = new Projectile(this.by, this.projectileType, this.targetType, this.target, this.durFly, this.projectilePath);
			projectile.damage = damage2;
			if (this.projectileImpactVis != null)
			{
				projectile.visualEffect = this.projectileImpactVis.GetCopy();
			}
			this.world.OnPreAttack(this.by, damage2, projectile);
			this.world.OnWeaponUsed(this.by);
			this.by.AddProjectile(projectile);
		}

		public override bool IsActive()
		{
			return this.isActive;
		}

		public override void TryActivate()
		{
			if (this.immediatelyTarget == null && this.waitTime < this.durWait)
			{
				return;
			}
			if (this.load == 0)
			{
				this.isReloading = true;
			}
			if (this.isReloading)
			{
				this.Activate();
				return;
			}
			UnitHealthy unitHealthy = this.target;
			if (this.immediatelyTarget != null)
			{
				this.target = this.immediatelyTarget;
				this.immediatelyTarget = null;
			}
			else
			{
				if (this.target != null && this.target.IsAlive())
				{
					this.Activate();
					return;
				}
				this.target = this.world.GetRandomAliveEnemy();
			}
			if (this.target != null && this.target.IsAlive())
			{
				this.Activate();
			}
			if (unitHealthy != this.target)
			{
				base.OnAttackTargetChanged(unitHealthy, this.target);
			}
		}

		private void Activate()
		{
			this.isActive = true;
			this.waitTime = 0f;
			this.attackTime = 0f;
			this.atSound = 0;
		}

		public override void OnDied()
		{
			this.isActive = false;
			this.attackTime = 0f;
			this.waitTime = 0f;
			this.hasThrown = false;
		}

		public override void OnInterrupted()
		{
			this.attackTime = 0f;
			this.waitTime = 0f;
			this.reloadTime = 0f;
			this.hasThrown = false;
			this.isActive = false;
		}

		public override bool IsReloading()
		{
			return this.isReloading;
		}

		public override float GetReloadTimeRatio()
		{
			if (this.isReloading)
			{
				return this.reloadTime / this.durReload;
			}
			return -1f;
		}

		public override void AttackImmediately(UnitHealthy unit)
		{
			this.immediatelyTarget = this.target;
		}

		private float durAttack;

		private float durWait;

		private float damageMoment;

		public int loadMax;

		public float durReload;

		public Sound soundReload;

		public Projectile.Type projectileType;

		public Projectile.TargetType targetType;

		public float durFly;

		public ProjectilePath projectilePath;

		public VisualEffect projectileImpactVis;

		private bool isActive;

		private float attackTime;

		private float waitTime;

		private bool hasThrown;

		private int load;

		private bool isReloading;

		private float reloadTime;

		private UnitHealthy target;

		private UnitHealthy immediatelyTarget;
	}
}
