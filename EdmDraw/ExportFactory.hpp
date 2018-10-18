#include <UnionNXKit\UnSession.hpp>

namespace UnionNXKit
{
	//导出工厂
	/*
	  外部环境需要设置
	  UGII_BASE_DIR
	  UGII_ROOT_DIR
	  PATH=%UGII_ROOT_DIR%
	*/
	class ExportFactory 
	{
	public:
		ExportFactory();
		~ExportFactory();

		//导出IGES
		static void ExportIges(const char *inputFile,const char *outputFile);

		//导出STEP214
		static void ExportStep214(const char *inputFile,const char *outputFile);

		//导出JT
		static void ExportJT(const char *inputFile,const char *outputFile);
	};
};