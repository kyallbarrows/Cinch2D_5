using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using FarseerPhysics.Dynamics;

namespace Cinch2D
{
	/*! Basically a draggable DisplayObjectContainer, with some setup methods for initinalizing from a texture.  Also, easier to type, so, bonus. */
	public class CinchSprite : DisplayObjectContainer {
		
		/// <summary>
		/// The Farseer Physics Body.  If set, and the Sprite is added to a physics container, it will control the position and rotation of the Sprite.
		/// </summary>
		public Body PhysicsBody;
		
		protected bool _dragging = false;
		protected Vector2 _dragCenter;
		
		/// <summary>
		/// Starts dragging the object.
		/// </summary>
		/// <param name='lockCenter'>
		/// Centers object to mouse around registration point.
		/// </param>
		/// <exception cref='Exception'>
		/// Throws exception if not added to the stage yet.
		/// </exception>
		public virtual void StartDrag(bool lockCenter = false){
			if (Parent == null || Stage == null)
				throw new Exception("Cannot startDrag until added to stage");
			
			_dragging = true;
			_dragCenter = new Vector2(0, 0);
			if (!lockCenter)
			{
				_dragCenter = new Vector2(Parent.MouseX - X, Parent.MouseY - Y);
			}
		}
		
		/// <summary>
		/// Stops the drag.
		/// </summary>
		public virtual void StopDrag(){
			_dragging = false;
		}
		
		public override void __InternalOnEnterFrame(){
			base.__InternalOnEnterFrame();
			if(_dragging)
			{
				var offset = Parent.GlobalToLocal(new Vector2(Stage.Instance.MouseX, Stage.Instance.MouseY));
				SetPosition(offset.x - _dragCenter.x, offset.y - _dragCenter.y);
			}
			else if (PhysicsBody != null)
			{
				SetPosition(PhysicsBody.Position.x, PhysicsBody.Position.y, PhysicsBody.Rotation);
			}
		}
		
		protected Texture2D _texture;
		protected Sprite _sprite;
		protected SpriteRenderer _spriteRenderer;

		protected RegistrationPoint _regPoint;	
		
		/// <summary>
		/// Creates a new Sprite with the supplied texture.
		/// </summary>
		/// <returns>
		/// The new Sprite.
		/// </returns>
		/// <param name='texturePath'>
		/// Texture to be displayed in the Sprite.
		/// </param>
		/// <param name='pixelsPerMeter'>
		/// Pixels per meter.  Specifying 100 with a 100x100 texture will produce a 1x1 meter Sprite.  You can pass in CinchOptions.DefaultPixelsPerMeter to keep everything the same size.
		/// </param>
		/// <param name='regPoint'>
		/// The registration point (center to pivot around).  Defaults to center.  You can use any of the constants defined on the RegistrationPoint struct, new() up one.
		/// </param>
		public static CinchSprite NewFromImage(string texturePath, float? pixelsPerMeter = null, Vector2? regPoint = null)
		{
			GameObject go = new GameObject(texturePath);
			var sprite = go.AddComponent<CinchSprite>();
			
			sprite.InitFromImage(texturePath, pixelsPerMeter ?? 200f, regPoint);
			return sprite;
		}
		
		/// <summary>
		/// Creates a new Sprite from a portion of the supplied sprite sheet texture.  See http://www.codeandweb.com/what-is-a-sprite-sheet for more on sprite sheets.
		/// </summary>
		/// <returns>
		/// The new Sprite.
		/// </returns>
		public static CinchSprite NewFromSpriteSheet(string texturePath, float left, float top, float width, float height, float pixelsPerMeter = 0, Vector2? regPoint = null)
		{
			GameObject go = new GameObject(texturePath);
			var sprite = go.AddComponent<CinchSprite>();

			sprite.InitFromImage (texturePath, pixelsPerMeter, regPoint, new Rect(left, top, width, height));
			return sprite;
		}
		
		/// <summary>
		/// Creates a new empty Sprite.
		/// </summary>
		/// <returns>
		/// The new sprite.
		/// </returns>
		public static CinchSprite NewEmpty(string name)
		{
			GameObject go = new GameObject(name);
			var sprite = go.AddComponent<CinchSprite>();
			return sprite;
		}

		protected void InitFromImage (string texturePath, float pixelsPerMeter, Vector2? regPoint = null, Rect? rect = null)
		{
			_spriteRenderer = gameObject.AddComponent<SpriteRenderer> ();
			_texture = TextureCache.GetCachedTexture (texturePath);
			_sprite = Sprite.Create (_texture, 
			                         rect ?? new Rect (0, 0, _texture.width-1, _texture.height-1), 
			                         regPoint ?? RegistrationPoint.Center, 
			                         pixelsPerMeter);
			_spriteRenderer.sprite = _sprite;
		}
		


		/// <summary>
		/// Creates a circular mesh with the supplied radius and number of sections.  Useful in conjunction with DisplayObjectContainer.SetMouseArea()
		/// </summary>
		/// <returns>
		/// The round mesh.
		/// </returns>
		/// <param name='radius'>
		/// The radius of the new mesh.
		/// </param>
		/// <param name='numSlices'>
		/// Number slices.  5 will produce a pentagon, 8 a hexagon, etc.  10 is a high enough approximation for most sprites, high numbers will slow performance.
		/// </param>
		public static Mesh CreateRoundHitMesh(float radius, int numSlices)
		{
			var hitMesh = new Mesh();
			var vertices = new List<Vector3>();
			var triangles = new List<int>();
			var uv = new List<Vector2>();
			
			var garbageUV = new Vector2(0, 0);
			vertices.Add(new Vector3(0, 0, 0));
			vertices.Add(new Vector3(radius * Mathf.Cos (0), radius * Mathf.Sin (0), 0));
			uv.Add(garbageUV);
			uv.Add(garbageUV);
			for (var step = 1; step < numSlices; step++)
			{
				var theta = step * 360/numSlices * Mathf.Deg2Rad;
				var newVert = new Vector3(radius * Mathf.Cos (theta), radius * Mathf.Sin (theta), 0);
				vertices.Add(newVert);
				triangles.Add(0);
				triangles.Add(step-1);
				triangles.Add(step);
				uv.Add(garbageUV);
			}

			//add the last triangle
			triangles.Add(0);
			triangles.Add(numSlices-1);
			triangles.Add(1);
			
			hitMesh.vertices = vertices.ToArray();
			hitMesh.triangles = triangles.ToArray();
			hitMesh.uv = uv.ToArray();
			hitMesh.RecalculateNormals();
			
			return hitMesh;
		}
		
		public override float X { 
			get{return _x;} 
			set{
				_x = value; 
				if (PhysicsBody != null)
					PhysicsBody.Position = new Vector2(_x, _y);
				
				__UpdateMesh(); 
				NotifyParentMeshInvalid();
			}}
		public override float Y { 
			get{return _y;} 
			set{
				_y = value; 
				if (PhysicsBody != null)
					PhysicsBody.Position = new Vector2(_x, _y);
				
				__UpdateMesh(); 
				NotifyParentMeshInvalid();
			}}
		public override void SetPosition (float x, float y, float rotation)
		{
			if (PhysicsBody != null)
			{
				PhysicsBody.Position = new Vector2(x, y);
				PhysicsBody.Rotation = rotation;
			}
			
			base.SetPosition (x, y, rotation);
		}
		
		public override float Rotation{
			get { return _rotation; }
			set { 
				_rotation = value; 
				if (PhysicsBody != null)
					PhysicsBody.Rotation = value;
				
				__UpdateMesh();
				NotifyParentMeshInvalid();
			}
		}	
	}
}