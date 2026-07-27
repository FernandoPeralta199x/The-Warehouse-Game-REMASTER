using TW08.Race;
using UnityEngine;

namespace TW08.PowerUps
{
    public readonly struct PowerUpContext
    {
        public readonly Transform User;
        public readonly ArcadeForkliftController2D Controller;
        public readonly ForkliftDamage Damage;
        public readonly LayerMask RacerLayers;

        public PowerUpContext(Transform user, ArcadeForkliftController2D controller, ForkliftDamage damage, LayerMask racerLayers)
        {
            User = user;
            Controller = controller;
            Damage = damage;
            RacerLayers = racerLayers;
        }
    }
}
