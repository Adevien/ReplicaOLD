using Replica.Structures;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StDevInterpolation : MonoBehaviour {

    public GameObject Server;
    public GameObject Client;

    float _lastSnapshot;

    float _cTimeLastSnapshotReceived;
    float _cTimeSinceLastSnapshotReceived;

    [SerializeField] float _cDelayTarget;
    [SerializeField] float _cRealDelayTarget;


    [SerializeField] float _cMaxServerTimeReceived;
    [SerializeField] float _cInterpolationTime;
    [SerializeField] float _cInterpTimeScale;

    [SerializeField] int SNAPSHOT_OFFSET_COUNT = 2;

    Queue<Snapshot> _cNetworkSimQueue = new Queue<Snapshot>();
    List<Snapshot> _cSnapshots = new List<Snapshot>();

    const int SNAPSHOT_RATE = 32;
    const float SNAPSHOT_INTERVAL = 1.0f / SNAPSHOT_RATE;

    [SerializeField] float INTERP_NEGATIVE_THRESHOLD;
    [SerializeField] float INTERP_POSITIVE_THRESHOLD;

    void Start() {
        _cInterpTimeScale = 1;

        _cSnapshotDeliveryDeltaAvg.Initialize(SNAPSHOT_RATE);
    }

    StdDev _cSnapshotDeliveryDeltaAvg;

    void Update() {
        ServerMovement();
        ServerSnapshot();

        ClientUpdateInterpolationTime();
        ClientReceiveDataFromServer();
        ClientRenderLatestPostion();
    }

    void ClientReceiveDataFromServer() {
        var received = false;

        while (_cNetworkSimQueue.Count > 0 && _cNetworkSimQueue.Peek().DeliveryTime < Time.time) {
            if (_cSnapshots.Count == 0)
                _cInterpolationTime = _cNetworkSimQueue.Peek().Time - SNAPSHOT_INTERVAL * SNAPSHOT_OFFSET_COUNT;

            var snapshot = _cNetworkSimQueue.Dequeue();

            _cSnapshots.Add(snapshot);
            _cMaxServerTimeReceived = Math.Max(_cMaxServerTimeReceived, snapshot.Time);

            received = true;
        }

        if (received) {
            _cSnapshotDeliveryDeltaAvg.Integrate(Time.time - _cTimeLastSnapshotReceived);
            _cTimeLastSnapshotReceived = Time.time;
            _cTimeSinceLastSnapshotReceived = 0f;

            _cDelayTarget = SNAPSHOT_INTERVAL * SNAPSHOT_OFFSET_COUNT + _cSnapshotDeliveryDeltaAvg.Mean + _cSnapshotDeliveryDeltaAvg.Value * 2f;
        }

        _cRealDelayTarget = _cMaxServerTimeReceived + _cTimeSinceLastSnapshotReceived - _cInterpolationTime - _cDelayTarget;

        if (_cRealDelayTarget > SNAPSHOT_INTERVAL * INTERP_POSITIVE_THRESHOLD)
            _cInterpTimeScale = 1.05f;
        else if (_cRealDelayTarget < SNAPSHOT_INTERVAL * -INTERP_NEGATIVE_THRESHOLD)
            _cInterpTimeScale = 0.95f;
        else _cInterpTimeScale = 1.0f;

        _cTimeSinceLastSnapshotReceived += Time.unscaledDeltaTime;
    }

    void ClientUpdateInterpolationTime() {
        _cInterpolationTime += Time.unscaledDeltaTime * _cInterpTimeScale;
    }

    void ClientRenderLatestPostion() {
        if (_cSnapshots.Count > 0) {
            var interpFrom = default(Vector3);
            var interpTo = default(Vector3);
            var interpAlpha = default(float);

            for (int i = 0; i < _cSnapshots.Count; ++i) {
                if (i + 1 == _cSnapshots.Count) {
                    if (_cSnapshots[0].Time > _cInterpolationTime) {
                        interpFrom = interpTo = _cSnapshots[0].Data[0].Position;
                        interpAlpha = 0;
                    } else {
                        interpFrom = interpTo = _cSnapshots[i].Data[0].Position;
                        interpAlpha = 0;
                    }
                } else {

                    var f = i;
                    var t = i + 1;

                    if (_cSnapshots[f].Time <= _cInterpolationTime && _cSnapshots[t].Time >= _cInterpolationTime) {
                        interpFrom = _cSnapshots[f].Data[0].Position;
                        interpTo = _cSnapshots[t].Data[0].Position;

                        var range = _cSnapshots[t].Time - _cSnapshots[f].Time;
                        var current = _cInterpolationTime - _cSnapshots[f].Time;

                        interpAlpha = Mathf.Clamp01(current / range);

                        break;
                    }
                }
            }

            Client.transform.position = Vector3.Lerp(interpFrom, interpTo, interpAlpha);
        }
    }

    void ServerMovement() {
        Vector3 pos;
        pos = Server.transform.position;
        pos.x = Mathf.PingPong(Time.time * 5, 10f) - 5f;

        Server.transform.position = pos;
    }

    [SerializeField, Range(0, 0.4f)] float random;

    void ServerSnapshot() {
        if (_lastSnapshot + Time.fixedDeltaTime < Time.time) {
            random = UnityEngine.Random.Range(0, 0.4f);

            _lastSnapshot = Time.time;

            var snaphot = new Snapshot {
                Time = _lastSnapshot,
                Data = new SnapshotData[1] { new SnapshotData() { Position = Server.transform.position } },
                DeliveryTime = Time.time + random
            };

            snaphot.Data[0].Position.y = 2;

            _cNetworkSimQueue.Enqueue(snaphot);


        }
    }
}