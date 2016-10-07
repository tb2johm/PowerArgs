﻿using PowerArgs.Cli;
using PowerArgs.Cli.Physics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleZombies
{
    public class MainCharacter : Thing, IDestructible
    {
        [ThreadStatic]
        private static MainCharacter _instance;
        public static MainCharacter Current
        {
            get
            {
                return _instance;
            }
            private set
            {
                if(_instance != null && value != null)
                {
                    throw new InvalidOperationException("There is already a main character in the game");
                }
                _instance = value;
            }
        }

        public SpeedTracker Speed { get; private set; }
        public Targeting Targeting { get; private set; }
        public Cursor FreeAimCursor { get; set; }
        public Thing Target { get; set; }
        public Inventory Inventory { get; set; } = new Inventory();
        public float HealthPoints { get; set; }
        public Event EatenByZombie { get; private set; } = new Event();
        public bool IsInLevelBuilder { get; set; }

       


        public MainCharacter()
        {
            Speed = new SpeedTracker(this);
            Targeting = new Targeting(this);
            Speed.Bounciness = 0;
            Speed.HitDetectionTypes.Add(typeof(Wall));

            Added.SubscribeForLifetime(OnAdded, this.LifetimeManager);
            Removed.SubscribeForLifetime(OnRemoved, this.LifetimeManager);
       }

        public void StartFreeAim()
        {
            var cursor = FreeAimCursor;
            if (cursor == null)
            {
                FreeAimCursor = new Cursor() { Bounds = Bounds.Clone() };

                if (Target != null && IsExpired == false)
                {
                    FreeAimCursor.Bounds.MoveTo(Target.Bounds.Location);
                    this.FreeAimCursor.RoundToNearestPixel();
                }

                Scene.Add(FreeAimCursor);
                Speed.SpeedX = 0;
                Speed.SpeedY = 0;
            }
            else
            {
                EndFreeAim();
            }
        }

        public void EndFreeAim()
        {
            Scene.Remove(FreeAimCursor);
            FreeAimCursor = null;
        }


        public void MoveLeft()
        {
            if (FreeAimCursor != null)
            {
                SceneHelpers.MoveThingSafeBy(Scene, FreeAimCursor, -1, 0);
                return;
            }

            if (Speed.SpeedX < 0 && Math.Abs(Speed.SpeedX) > Math.Abs(Speed.SpeedY))
            {
                Speed.SpeedX = 0;
                Speed.SpeedY = 0;
                this.RoundToNearestPixel();
            }
            else
            {
                Speed.SpeedX = -7;
                Speed.SpeedY = 0;
                this.RoundToNearestPixel();
            }
        }

        public void MoveDown()
        {
            if (FreeAimCursor != null)
            {
                SceneHelpers.MoveThingSafeBy(Scene, FreeAimCursor, 0, 1);
                return;
            }

            if (Speed.SpeedY > Math.Abs(Speed.SpeedX))
            {
                Speed.SpeedY = 0;
                Speed.SpeedX = 0;
                this.RoundToNearestPixel();

            }
            else
            {
                Speed.SpeedY = 7;
                Speed.SpeedX = 0;
                this.RoundToNearestPixel();
            }
        }

        public void MoveUp()
        {
            if (FreeAimCursor != null)
            {
                SceneHelpers.MoveThingSafeBy(Scene, FreeAimCursor, 0, -1);
                return;
            }

            if (Speed.SpeedY < 0 && Math.Abs(Speed.SpeedY) > Math.Abs(Speed.SpeedX))
            {
                Speed.SpeedY = 0;
                Speed.SpeedX = 0;
                this.RoundToNearestPixel();
            }
            else
            {
                Speed.SpeedY = -7;
                Speed.SpeedX = 0;
                this.RoundToNearestPixel();
            }
        }


        public void MoveRight()
        {
            if (FreeAimCursor != null)
            {
                SceneHelpers.MoveThingSafeBy(Scene, FreeAimCursor, 1, 0);
                return;
            }

            if (Speed.SpeedX > Math.Abs(Speed.SpeedY))
            {
                Speed.SpeedX = 0;
                Speed.SpeedY = 0;
                this.RoundToNearestPixel();
            }
            else
            {
                Speed.SpeedX = 7;
                Speed.SpeedY = 0;
                this.RoundToNearestPixel();
            }
        }

        public void TryOpenCloseDoor()
        {
            var door = (Door)Scene.Things
                   .Where(t => t is Door).OrderBy(d => Bounds.Location.CalculateDistanceTo(d.Bounds.Location)).FirstOrDefault();

            if (door != null && Bounds.Location.CalculateDistanceTo(door.Bounds.Location) <= 2.5)
            {
                door.IsOpen = !door.IsOpen;

                if (SceneHelpers.GetThingsITouch(Scene, this, new List<Type>() { typeof(Door) }).Contains(door))
                {
                    if (door.IsOpen)
                    {
                        Bounds.MoveTo(door.ClosedBounds.Location);
                    }
                    else
                    {
                        Bounds.MoveTo(door.OpenLocation);
                    }
                }

                Scene.Update(door);
                Scene.Update(this);
            }
        }

        private void OnAdded()
        {
            Current = this;
        }

        private void OnRemoved()
        {
            Current = null;
        }
    }

    [ThingBinding(typeof(MainCharacter))]
    public class MainCharacterRenderer : ThingRenderer
    {
        public MainCharacterRenderer()
        {
            this.TransparentBackground = true;
            ZIndex = 2000;
        }

        protected override void OnPaint(ConsoleBitmap context)
        {
            base.OnPaint(context);
            context.Pen = new PowerArgs.ConsoleCharacter('X', ConsoleColor.Green);
            context.FillRect(0, 0,Width,Height);
        }
    }
}