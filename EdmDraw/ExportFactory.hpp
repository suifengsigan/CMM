#include <UnionNXKit\UnSession.hpp>

namespace UnionNXKit
{
	//��������
	/*
	  �ⲿ������Ҫ����
	  UGII_BASE_DIR
	  UGII_ROOT_DIR
	  PATH=%UGII_ROOT_DIR%
	*/
	class ExportFactory 
	{
	public:
		ExportFactory();
		~ExportFactory();

		//����IGES
		static void ExportIges(const char *inputFile,const char *outputFile);

		//����STEP214
		static void ExportStep214(const char *inputFile,const char *outputFile);

		//����JT
		static void ExportJT(const char *inputFile,const char *outputFile);
	};
};