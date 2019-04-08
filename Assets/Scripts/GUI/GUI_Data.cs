using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Simuro5v5;


/*
 *  public class MatchInfo
    {
        private Robot[] blueRobot = new Robot[5];
        private Robot[] yellowRobot = new Robot[5];
        private Ball currentBall;
        private int gameState;
        private int whosBall;

        public Robot[] BlueRobot { get { return blueRobot; } }
        public Robot[] YellowRobot { get { return yellowRobot; } }
        public Ball CurrentBall { get { return currentBall; } }
        public int GameState { get { return gameState; } }
        public int WhosBall { get { return whosBall; } }

        public MatchInfo() { }
        public MatchInfo(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {
            Update(ball, blue, yellow);
        }

        // 从GameObject中获取数据来更新
        // 将Unity系统里刚体以x,z分量转换成惯用的x,y分量描述二维矢量
        //      包括：坐标，线速度，车的角速度
        // 将Unity系统里刚体以x,z,y分量转换成惯用的x,y,z分量描述二维矢量
        //      包括：球的角速度
        // 规定 rot属于(-180, 180]
        public void Update(GameObject ball, GameObject[] blue, GameObject[] yellow)
        {
            
            for (int i = 0; i < 5; i++)
            {
                Rigidbody blueTemp = blue[i].GetComponent<Rigidbody>();
                BlueRobot[i].mass = blueTemp.mass;
                BlueRobot[i].pos.x = blueTemp.position.x;
                BlueRobot[i].pos.y = blueTemp.position.z;
                BlueRobot[i].rotation = blueTemp.rotation.eulerAngles.y.FormatUnity2OldStd();
                Console.WriteLine("rot0=" + blueTemp.rotation.eulerAngles.y + "\n" + "rot1=" + BlueRobot[i].rotation);// 调试
                BlueRobot[i].linearVelocity.x = blueTemp.velocity.x;
                BlueRobot[i].linearVelocity.y = blueTemp.velocity.z;
                BlueRobot[i].angularVelocity.x = blueTemp.angularVelocity.x;
                BlueRobot[i].angularVelocity.y = blueTemp.angularVelocity.z;

                Rigidbody yellowTemp = yellow[i].GetComponent<Rigidbody>();
                YellowRobot[i].mass = yellowTemp.mass;
                YellowRobot[i].pos.x = yellowTemp.position.x;
                YellowRobot[i].pos.y = yellowTemp.position.z;
                YellowRobot[i].rotation = yellowTemp.rotation.eulerAngles.y.FormatUnity2OldStd();
                YellowRobot[i].linearVelocity.x = yellowTemp.velocity.x;
                YellowRobot[i].linearVelocity.y = yellowTemp.velocity.z;
                YellowRobot[i].angularVelocity.x = yellowTemp.angularVelocity.x;
                YellowRobot[i].angularVelocity.y = yellowTemp.angularVelocity.z;
            }

            Rigidbody ballTemp = ball.GetComponent<Rigidbody>();
            currentBall.mass = ballTemp.mass;
            //currentBall.pos.x = ball.transform.position.x;
            //currentBall.pos.y = ball.transform.position.z;
            currentBall.pos.x = ballTemp.position.x;
            currentBall.pos.y = ballTemp.position.z;
            currentBall.linearVelocity.x = ballTemp.velocity.x;
            currentBall.linearVelocity.y = ballTemp.velocity.z;
            currentBall.angularVelocity.x = ballTemp.angularVelocity.x;
            currentBall.angularVelocity.y = ballTemp.angularVelocity.z;
            currentBall.angularVelocity.z = ballTemp.angularVelocity.y;
        }
 */

public class GUI_Data : MonoBehaviour {

    //List <MatchInfo> GUI_Replay_Match

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
