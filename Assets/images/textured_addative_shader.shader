Shader "Custom/textured_addative_shader" 
{
   Properties 
   {
      _MainTex   ("Texture Image", 2D)  = "white" {} 
      _MainColor ("Color", Color)       = (1,1,1,1)
   }
   SubShader 
   {
      Tags {"Queue" = "Transparent"} 

      Pass 
      {  
         Cull   Back  // now render the front faces
         ZWrite Off   // don't write to depth buffer 
                      // in order not to occlude other objects
                      
         Blend SrcAlpha One
            // blend based on the fragment's alpha value
 
         GLSLPROGRAM
 
         uniform sampler2D  _MainTex;
         uniform vec4       _MainColor;
 
         varying vec4 textureCoordinates; 
 
         #ifdef VERTEX
 
         void main()
         {
            textureCoordinates = gl_MultiTexCoord0;
            gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
         }
 
         #endif
 
         #ifdef FRAGMENT
 
         void main()
         {
            vec4 textureColor = texture2D(_MainTex, vec2(textureCoordinates));
         
            gl_FragColor.rgb  = _MainColor.rgb;
            gl_FragColor.a    = textureColor.a;
         }
 
         #endif
 
         ENDGLSL
      }
   }
   // The definition of a fallback shader should be commented out 
   // during development:
   // Fallback "Unlit/Transparent"
}