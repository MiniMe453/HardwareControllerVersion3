using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AlekGames.Placer.Profiles;

namespace AlekGames.Placer.Shared
{
    public interface IPrefabPalleteUser
    {
        void setPalette(prefabPalette pallete);
        prefabPalette getPalette();

        void setSpecificIndex(int index);
        int getSpecificIndex();
    }
}
