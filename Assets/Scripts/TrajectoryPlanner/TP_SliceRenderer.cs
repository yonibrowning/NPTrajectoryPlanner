using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using TrajectoryPlanner;
using UnityEngine.Serialization;

public class TP_SliceRenderer : MonoBehaviour
{
    [FormerlySerializedAs("sagittalSliceGO")] [SerializeField] private GameObject _sagittalSliceGo;
    [FormerlySerializedAs("coronalSliceGO")] [SerializeField] private GameObject _coronalSliceGo;
    [FormerlySerializedAs("tpmanager")] [SerializeField] private TrajectoryPlannerManager _tpmanager;
    [FormerlySerializedAs("modelControl")] [SerializeField] private CCFModelControl _modelControl;
    [FormerlySerializedAs("inPlaneSlice")] [SerializeField] private TP_InPlaneSlice _inPlaneSlice;
    [FormerlySerializedAs("util")] [SerializeField] private TP_Utils _util;
    [FormerlySerializedAs("dropdownMenu")] [SerializeField] private TMP_Dropdown _dropdownMenu;
    [FormerlySerializedAs("iblCoverageTextureAssetRef")] [SerializeField] private AssetReference _iblCoverageTextureAssetRef;

    private bool loaded;

    private bool camXLeft;
    private bool camYBack;

    private Material saggitalSliceMaterial;
    private Material coronalSliceMaterial;

    private void Awake()
    {
        saggitalSliceMaterial = _sagittalSliceGo.GetComponent<Renderer>().material;
        coronalSliceMaterial = _coronalSliceGo.GetComponent<Renderer>().material;

        loaded = false;
    }

    // Start is called before the first frame update
    private async void Start()
    {
        Debug.Log("(SliceRenderer) Waiting for inplane slice to complete");
        await _inPlaneSlice.GetGPUTextureTask();
        Debug.Log("(SliceRenderer) Waiting for node models to load");
        await _modelControl.GetDefaultLoadedTask();

        if (_dropdownMenu.value == 1)
            SetActiveTextureAnnotation();
        //else if (dropdownMenu.value == 2)
        //    SetActiveTextureIBLCoverage();
        loaded = true;
    }


    //private Texture3D iblCoverageTexture;
    //private bool coverageLoaded;
    //private bool coverageLoading;

    //public async void LoadCoverageTexture()
    //{
    //    coverageLoading = true;

    //    if (iblCoverageTextureAssetRef == null)
    //    {
    //        VolumetricDataset coverageDataset = tpmanager.GetIBLCoverageDataset();
    //        if (coverageDataset == null)
    //        {
    //            await tpmanager.LoadIBLCoverageDataset();
    //            coverageDataset = tpmanager.GetIBLCoverageDataset();
    //        }
    //        AnnotationDataset annotationDataset = tpmanager.GetAnnotationDataset();

    //        Color[] colors = new Color[] { Color.grey, Color.yellow, Color.green };

    //        iblCoverageTexture = new Texture3D(528, 320, 456, TextureFormat.RGB24, false);
    //        iblCoverageTexture.filterMode = FilterMode.Point;
    //        iblCoverageTexture.wrapMode = TextureWrapMode.Clamp;

    //        Debug.Log("Converting annotation dataset to texture format");
    //        for (int ap = 0; ap < 528; ap++)
    //        {
    //            for (int dv = 0; dv < 320; dv++)
    //                for (int ml = 0; ml < 456; ml++)
    //                    if (annotationDataset.ValueAtIndex(ap, dv, ml) > 0)
    //                        iblCoverageTexture.SetPixel(ap, dv, ml, colors[coverageDataset.ValueAtIndex(ap, dv, ml)]);
    //                    else
    //                        iblCoverageTexture.SetPixel(ap, dv, ml, Color.black);
    //        }
    //        iblCoverageTexture.Apply();

    //        //if (Application.isEditor)
    //        //    AssetDatabase.CreateAsset(iblCoverageTexture, "Assets/AddressableAssets/Textures/IBLCoverageTexture3D.asset");
    //    }
    //    else
    //    {
    //        AsyncOperationHandle<Texture3D> dataLoader = iblCoverageTextureAssetRef.LoadAssetAsync<Texture3D>();
    //        await dataLoader.Task;
    //        iblCoverageTexture = dataLoader.Result;
    //    }
    //    coverageLoaded = true;

    //    SetActiveTextureIBLCoverage();
    //}

    private async void SetActiveTextureAnnotation()
    {
        Task<Texture3D> textureTask = _inPlaneSlice.GetAnnotationDatasetGPUTexture();
        await textureTask;

        saggitalSliceMaterial.SetTexture("_Volume", textureTask.Result);
        coronalSliceMaterial.SetTexture("_Volume", textureTask.Result);
    }

    //private void SetActiveTextureIBLCoverage()
    //{
    //    if (!coverageLoaded && !coverageLoading)
    //    {
    //        LoadCoverageTexture();
    //        return;
    //    }
    //    saggitalSliceMaterial.SetTexture("_Volume", iblCoverageTexture);
    //    coronalSliceMaterial.SetTexture("_Volume", iblCoverageTexture);
    //}

    private void Update()
    {
        if (Settings.Slice3DDropdownOption>0 && loaded)
        {
            // Check if the camera moved such that we have to flip the slice quads
            UpdateCameraPosition();
        }
    }

    private float apWorldmm;
    private float mlWorldmm;
    
    /// <summary>
    /// Shift the position of the sagittal and coronal slices to match the tip of the active probe
    /// </summary>
    public void UpdateSlicePosition()
    {
        if (Settings.Slice3DDropdownOption > 0)
        {
            if (ProbeManager.ActiveProbeManager == null) return;

            // the actual tip
            Vector3 probeTipWorld = ProbeManager.ActiveProbeManager.GetProbeController().ProbeTipT.position;
            // position the slices along the real tip in world space
            _coronalSliceGo.transform.position = new Vector3(0f, 0f, probeTipWorld.z);
            _sagittalSliceGo.transform.position = new Vector3(probeTipWorld.x, 0f, 0f);

            // for CCF coordinates
            (Vector3 tipCoordWorld, _, _) = ProbeManager.ActiveProbeManager.GetProbeController().GetTipWorldU();

            apWorldmm = tipCoordWorld.z + 6.6f;
            coronalSliceMaterial.SetFloat("_SlicePosition", apWorldmm / 13.2f);

            mlWorldmm = -(tipCoordWorld.x - 5.7f);
            saggitalSliceMaterial.SetFloat("_SlicePosition", mlWorldmm / 11.4f);

            UpdateNodeModelSlicing();
        }
    }

    private void UpdateCameraPosition()
    {
        Vector3 camPosition = Camera.main.transform.position;
        bool changed = false;
        if (camXLeft && camPosition.x < 0)
        {
            camXLeft = false;
            changed = true;
        }
        else if (!camXLeft && camPosition.x > 0)
        {
            camXLeft = true;
            changed = true;
        }
        else if (camYBack && camPosition.z < 0)
        {
            camYBack = false;
            changed = true;
        }
        else if (!camYBack && camPosition.z > 0)
        {
            camYBack = true;
            changed = true;
        }
        if (changed)
            UpdateNodeModelSlicing();
    }

    private void UpdateNodeModelSlicing()
    {
        // Update the renderers on the node objects
        foreach (CCFTreeNode node in _modelControl.GetDefaultLoadedNodes())
        {
            if (camYBack)
                // clip from apPosition forward
                node.SetShaderProperty("_APClip", new Vector2(0f, apWorldmm));
            else
                node.SetShaderProperty("_APClip", new Vector2(apWorldmm, 13.2f));

            if (camXLeft)
                // clip from mlPosition forward
                node.SetShaderProperty("_MLClip", new Vector2(mlWorldmm, 11.4f));
            else
                node.SetShaderProperty("_MLClip", new Vector2(0f, mlWorldmm));
        }
    }

    private void ClearNodeModelSlicing()
    {
        // Update the renderers on the node objects
        foreach (CCFTreeNode node in _modelControl.GetDefaultLoadedNodes())
        {
            node.SetShaderProperty("_APClip", new Vector2(0f, 13.2f));
            node.SetShaderProperty("_MLClip", new Vector2(0f, 11.4f));
        }
    }

    public void ToggleSliceVisibility(int sliceType)
    {
        if (sliceType==0)
        {
            // make slices invisible
            _sagittalSliceGo.SetActive(false);
            _coronalSliceGo.SetActive(false);
            ClearNodeModelSlicing();
        }
        else
        {
            // Standard sagittal/coronal slices
            _sagittalSliceGo.SetActive(true);
            _coronalSliceGo.SetActive(true);

            if (sliceType == 1)
                SetActiveTextureAnnotation();
            //else if (sliceType == 2)
            //    SetActiveTextureIBLCoverage();

            UpdateSlicePosition();
            UpdateCameraPosition();
        }
    }
}
