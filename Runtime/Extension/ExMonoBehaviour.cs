using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace JJFramework.Runtime.Extension
{
    [DisallowMultipleComponent]
    public abstract class ExMonoBehaviour : MonoBehaviour
    {
        private Animation _animation;
        public new Animation animation => _animation ? _animation : _animation = GetComponent<Animation>();

        private AudioSource _audio;
        public new AudioSource audio => _audio ? _audio : _audio = GetComponent<AudioSource>();

        private Collider _collider;
        public new Collider collider => _collider ? _collider : _collider = GetComponent<Collider>();

        private Collider2D _collider2D;
        public new Collider2D collider2D => _collider2D ? _collider2D : _collider2D = GetComponent<Collider2D>();

        private ConstantForce _constantForce;
        public new ConstantForce constantForce =>
            _constantForce ? _constantForce : _constantForce = GetComponent<ConstantForce>();

        private Transform _transform;
        public new Transform transform => _transform ? _transform : _transform = GetComponent<Transform>();

        private RectTransform _rectTransform;
        public RectTransform rectTransform =>
            _rectTransform ? _rectTransform : _rectTransform = GetComponent<RectTransform>();

        private ParticleSystem _particleSystem;
        public new ParticleSystem particleSystem =>
            _particleSystem ? _particleSystem : _particleSystem = GetComponent<ParticleSystem>();

        private Renderer _renderer;
        public new Renderer renderer => _renderer ? _renderer : _renderer = GetComponent<Renderer>();

        private Rigidbody _rigidbody;
        public new Rigidbody rigidbody => _rigidbody ? _rigidbody : _rigidbody = GetComponent<Rigidbody>();

        private Rigidbody2D _rigidbody2D;
        public new Rigidbody2D rigidbody2D => _rigidbody2D ? _rigidbody2D : _rigidbody2D = GetComponent<Rigidbody2D>();

        protected virtual void Awake()
        {
            this.LoadComponents();
        }
    }
}
