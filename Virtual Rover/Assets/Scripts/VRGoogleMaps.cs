using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRGoogleMaps : MonoBehaviour
{
    public string APIKEY = "";
    public double lat = 40.081905;
    public double lon = -75.163502;
    public float width = 640;
    public float height = 640;
    public float zoom = 14;

    public enum mapType { roadmap, satellite, hybrid, terrain };
    public mapType mapSelected;
    public int scale;

    string url = "";
    LocationInfo gps;
    public bool loadingMap = false;
    private IEnumerator mapCoroutine;

    private double pi = 3.1415926535897932;
    IEnumerator GetGoogleMap(double lat, double lon)
    {
        url = "https://maps.googleapis.com/maps/api/staticmap?center="
        + lat + "," + lon + "&zoom=" + zoom + "&size=" + width + "x" + height + "&scale=" + scale+ "&maptype=" + mapSelected +
        "&key=" + APIKEY;

        url = "https://www.google.com/maps/embed/v1/place"
                + "?key=" + APIKEY
                + "&q=Eiffel+Tower,Paris+France";
        loadingMap = true;
        WWW www = new WWW(url);
        yield return www;
        loadingMap = false;

        gameObject.GetComponent<RawImage>().texture = www.texture;

        StopCoroutine(mapCoroutine);
    }

    // Start is called before the first frame update
    void Start()
    {
        mapCoroutine = GetGoogleMap(lat, lon);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("New Map");
            lat = 40.081905;
            lon = -75.163502;
            mapCoroutine = GetGoogleMap(lat, lon);
            StartCoroutine(mapCoroutine);
        }
    }

    void calculateLocation()
    {
        double x = width / 2;
        double y = height / 2;
        double s = (double)Mathf.Min(Mathf.Max(Mathf.Sin((float)(lat * (pi / 180.00))), (float)-.9999), (float).9999);
    }
    
}
