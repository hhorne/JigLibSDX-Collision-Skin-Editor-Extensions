xof 0303txt 0032


template VertexDuplicationIndices { 
 <b8d65549-d7c9-4995-89cf-53a9a8b031e3>
 DWORD nIndices;
 DWORD nOriginalVertices;
 array DWORD indices[nIndices];
}
template XSkinMeshHeader {
 <3cf169ce-ff7c-44ab-93c0-f78f62d172e2>
 WORD nMaxSkinWeightsPerVertex;
 WORD nMaxSkinWeightsPerFace;
 WORD nBones;
}
template SkinWeights {
 <6f0d123b-bad2-4167-a0d0-80224f25fabb>
 STRING transformNodeName;
 DWORD nWeights;
 array DWORD vertexIndices[nWeights];
 array float weights[nWeights];
 Matrix4x4 matrixOffset;
}

Frame RootFrame {

  FrameTransformMatrix {
    1.000000,0.000000,0.000000,0.000000,
    0.000000,-0.000000,1.000000,0.000000,
    0.000000,1.000000,0.000000,0.000000,
    0.000000,0.000000,0.000000,1.000000;;
  }
  Frame Cube {

    FrameTransformMatrix {
      1.000000,0.000000,0.000000,0.000000,
      0.000000,1.000000,0.000000,0.000000,
      0.000000,0.000000,1.000000,0.000000,
      0.000000,0.000000,0.000000,1.000000;;
    }
    Mesh { //Cube
      24;
      0.500000; 0.500000; -0.500000;,
      0.500000; -0.500000; -0.500000;,
      -0.500000; -0.500000; -0.500000;,
      -0.500000; 0.500000; -0.500000;,
      0.500000; 0.500000; 0.500000;,
      -0.500000; 0.500000; 0.500000;,
      -0.500000; -0.500000; 0.500000;,
      0.500000; -0.500000; 0.500000;,
      0.500000; 0.500000; -0.500000;,
      0.500000; 0.500000; 0.500000;,
      0.500000; -0.500000; 0.500000;,
      0.500000; -0.500000; -0.500000;,
      0.500000; -0.500000; -0.500000;,
      0.500000; -0.500000; 0.500000;,
      -0.500000; -0.500000; 0.500000;,
      -0.500000; -0.500000; -0.500000;,
      -0.500000; -0.500000; -0.500000;,
      -0.500000; -0.500000; 0.500000;,
      -0.500000; 0.500000; 0.500000;,
      -0.500000; 0.500000; -0.500000;,
      0.500000; 0.500000; 0.500000;,
      0.500000; 0.500000; -0.500000;,
      -0.500000; 0.500000; -0.500000;,
      -0.500000; 0.500000; 0.500000;;
      6;
      4; 3, 2, 1, 0;,
      4; 7, 6, 5, 4;,
      4; 11, 10, 9, 8;,
      4; 15, 14, 13, 12;,
      4; 19, 18, 17, 16;,
      4; 23, 22, 21, 20;;
      MeshMaterialList {
        1;
        6;
        0,
        0,
        0,
        0,
        0,
        0;
        Material Material001 {
            1.000000; 1.000000; 1.000000;        1.0;;
            0.500000;
            1.000000; 1.000000; 1.000000;;
            0.0; 0.0; 0.0;;
        }  //End of Material
      }  //End of MeshMaterialList
      MeshNormals {
        24;
        0.577349; 0.577349; -0.577349;,
        0.577349; -0.577349; -0.577349;,
        -0.577349; -0.577349; -0.577349;,
        -0.577349; 0.577349; -0.577349;,
        0.577349; 0.577349; 0.577349;,
        -0.577349; 0.577349; 0.577349;,
        -0.577349; -0.577349; 0.577349;,
        0.577349; -0.577349; 0.577349;,
        0.577349; 0.577349; -0.577349;,
        0.577349; 0.577349; 0.577349;,
        0.577349; -0.577349; 0.577349;,
        0.577349; -0.577349; -0.577349;,
        0.577349; -0.577349; -0.577349;,
        0.577349; -0.577349; 0.577349;,
        -0.577349; -0.577349; 0.577349;,
        -0.577349; -0.577349; -0.577349;,
        -0.577349; -0.577349; -0.577349;,
        -0.577349; -0.577349; 0.577349;,
        -0.577349; 0.577349; 0.577349;,
        -0.577349; 0.577349; -0.577349;,
        0.577349; 0.577349; 0.577349;,
        0.577349; 0.577349; -0.577349;,
        -0.577349; 0.577349; -0.577349;,
        -0.577349; 0.577349; 0.577349;;
        6;
        4; 3, 2, 1, 0;,
        4; 7, 6, 5, 4;,
        4; 11, 10, 9, 8;,
        4; 15, 14, 13, 12;,
        4; 19, 18, 17, 16;,
        4; 23, 22, 21, 20;;
      }  //End of MeshNormals
    } // End of Mesh
  }  // End of the Object Cube 
}  // End of the Root Frame
