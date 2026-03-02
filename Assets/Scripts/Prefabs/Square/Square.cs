using HelpfulScripts;
using Services.Monobehs;
using UIUtils;
using UnityEngine;
using UnityEngine.UI;

namespace Prefabs
{
    public abstract class Square : MonoBehaviour
    {
        [SerializeField] protected ButtonListener _button;
        
        [SerializeField] protected Vector3 _scaleToOnSelect;
        [SerializeField] protected float _durationOfScaleRevert;

        [SerializeField] protected RectTransform _animationHandler;
        
        [SerializeField] private Image _backgroundSprite;
        
        protected Color _color;
        
        public virtual void Init(Color color)
        {
            _backgroundSprite.color = color;
            _color = color;
        }
    }
}