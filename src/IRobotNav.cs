using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotNav
{
	public interface IRobotNav
	{
		string Filename { get; set; }
		string Method { get; set; }

		void Init();
		void Exit();
	}
}
