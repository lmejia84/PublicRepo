import requests
from requests.exceptions import RequestException
import logging
from datetime import datetime
import time

'''
    Autor:      @Luis Mejia
    Propósito:  Grabar los programas de los lideres y ayudar de backup por si las grabaciones del chef fallan.
    Licencia:   The MIT License
'''
#Iniciar el log de errores
logging.basicConfig(filename=r'C:\\Users\\lmeji\\OneDrive\\LHT\\lht_errors.log', level=logging.ERROR)

stream_url = r'http://ice42.securenetsystems.net/VOXFM'
morning_start = '075500'
morning_end = '101500'
# morning_end = '150000'

evening_start = '175500'
evening_end = '201500'
now = datetime.now().strftime('%H%M%S')
record_stream = True


#dependiendo la hora graba el archivo en su respectiva carpeta, solo durante las horas del show
if (now >= morning_start and now <= morning_end):
    output_directory = r'C:\\Users\\lmeji\\OneDrive\\LHT\\LHDD\\'
elif (now >= evening_start and now <= evening_end):
    output_directory = r'C:\\Users\\lmeji\\OneDrive\\LHT\\LHT\\'
else:
    print("No hay programa ahorita")
    exit()

while record_stream: #Si hay problema de red trata de conectarse otra vez y grabar en un nuevo archivo        
    try:
        if (now >= morning_start and now <= morning_end):
            output_file = output_directory + datetime.now().strftime('LHDD-%Y%m%d%H%M%S.mp3')
        elif (now >= evening_start and now <= evening_end):
            output_file = output_directory + datetime.now().strftime('LHT-%Y%m%d%H%M%S.mp3')

        response = requests.get(stream_url, stream=True)
        response.raise_for_status()

        print(f"Conectado, grabando a los lideres en {output_file}.")

        with open(output_file, 'wb') as f:
            try:
                for block in response.iter_content(1024):
                    f.write(block)
                    outside_stream_hours = datetime.now().strftime('%H%M%S')
                    if outside_stream_hours >= morning_start and outside_stream_hours <= morning_end or outside_stream_hours >= evening_start and outside_stream_hours <= evening_end:
                        record_stream = True
                    else:
                        exit()               
            except RequestException as e:
                #Errores de red mientras está grabando
                error_time = datetime.now().strftime('%Y%m%d%H%M%S')
                print(f"{error_time}: Network error occurred during streaming: {e}")
                logging.error(f"{error_time}: Network error occurred during streaming: {e}")
            except Exception as e:
                error_time = datetime.now().strftime('%Y%m%d%H%M%S')
                print(f"{error_time}: An error occurred during streaming: {e}")
                logging.error(f"{error_time}: An error occurred during streaming: {e}")

    except RequestException as e:
        #Errores tratando de conectarse a la radio (Por ejemplo, cuando el stream link está down.)
        error_time = datetime.now().strftime('%Y%m%d%H%M%S')
        print(f"{error_time}: Network error occurred when trying to connect to stream: {e}")
        logging.error(f"{error_time}: Network error occurred when trying to connect to stream: {e}")

    except Exception as e:
        error_time = datetime.now().strftime('%Y%m%d%H%M%S')
        print(f"{error_time}: An error occurred when trying to connect to stream: {e}")
        logging.error(f"{error_time}: An error occurred when trying to connect to stream: {e}")

    finally:
        #Espera 2 segundos antes de tratar the conectarse otra vez.
        time.sleep(2)