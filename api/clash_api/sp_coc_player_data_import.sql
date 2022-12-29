CREATE OR REPLACE PROCEDURE `fleet-parser-330316.luistest.sp_coc_player_data_import`()
OPTIONS (description="This stored procedure inserts all the data from staging tables retrieved from the Clash of Clans API into the prod tables.")
BEGIN
##Insert Player Stats START 
  MERGE INTO `fleet-parser-330316.luistest.coc_player_stats` as t
  USING(
    SELECT
      tag                       as player_tag,
      name                      as player_name,
      townHallLevel             as townhall_level,
      townHallWeaponLevel       as eagle_artillery,
      expLevel                  as exp_level,
      trophies                  as player_trophies,
      bestTrophies              as player_best_trophies,
      warStars                  as war_stars,
      attackWins                as attack_wins,
      defenseWins               as defense_wins,
      versusBattleWinCount      as verus_battle_wins,  
      builderHallLevel          as builderhall_level,
      versusTrophies            as versus_trophies,
      bestVersusTrophies        as best_versus_trophies,
      versusBattleWins          as versus_battle_wins,
      role                      as clan_role,
      warPreference             as war_preference,
      donations                 as player_donations,
      donationsReceived         as player_donations_received,
      clanCapitalContributions  as clan_capital_contributions,
      clan.badgeUrls.large      as clan_badge_url,
      clan.clanLevel            as clan_level,
      clan.name                 as clan_name,
      clan.tag                  as clan_tag,
      archive_date              as archive_date,
    FROM `fleet-parser-330316.luis_stage.stg_coc_player_stats`
  ) as s
  on s.archive_date = t.archive_date
  and s.player_tag = t.player_tag

  WHEN MATCHED THEN UPDATE SET
      t.player_name = s.player_name,
      t.townhall_level = s.townhall_level,
      t.eagle_artillery = s.eagle_artillery,
      t.exp_level = s.exp_level,
      t.player_trophies = s.player_trophies,
      t.player_best_trophies = s.player_best_trophies,
      t.war_stars = s.war_stars,
      t.attack_wins = s.attack_wins,
      t.defense_wins = s.defense_wins,
      t.verus_battle_wins  = s.verus_battle_wins,
      t.builderhall_level = s.builderhall_level,
      t.versus_trophies = s.versus_trophies,
      t.best_versus_trophies = s.best_versus_trophies,
      t.versus_battle_wins = s.versus_battle_wins,
      t.clan_role = s.clan_role,
      t.war_preference = s.war_preference,
      t.player_donations = s.player_donations,
      t.player_donations_received = s.player_donations_received,
      t.clan_capital_contributions = s.clan_capital_contributions,
      t.clan_badge_url = s.clan_badge_url,
      t.clan_level = s.clan_level,
      t.clan_name = s.clan_name,
      t.clan_tag = s.clan_tag

  WHEN NOT MATCHED THEN INSERT
      (
        player_tag,
        player_name,
        townhall_level,
        eagle_artillery,
        exp_level,
        player_trophies,
        player_best_trophies,
        war_stars,
        attack_wins,
        defense_wins,
        verus_battle_wins,  
        builderhall_level,
        versus_trophies,
        best_versus_trophies,
        versus_battle_wins,
        clan_role,
        war_preference,
        player_donations,
        player_donations_received,
        clan_capital_contributions,
        clan_badge_url,
        clan_level,
        clan_name,
        clan_tag,
        archive_date
      )    
  VALUES
      (
        s.player_tag,
        s.player_name,
        s.townhall_level,
        s.eagle_artillery,
        s.exp_level,
        s.player_trophies,
        s.player_best_trophies,
        s.war_stars,
        s.attack_wins,
        s.defense_wins,
        s.verus_battle_wins,  
        s.builderhall_level,
        s.versus_trophies,
        s.best_versus_trophies,
        s.versus_battle_wins,
        s.clan_role,
        s.war_preference,
        s.player_donations,
        s.player_donations_received,
        s.clan_capital_contributions,
        s.clan_badge_url,
        s.clan_level,
        s.clan_name,
        s.clan_tag,
        s.archive_date
      );
##Insert Player Stats END

##Insert Hero data START
  MERGE INTO`fleet-parser-330316.luistest.coc_player_heroes` as t
  USING (
    SELECT
      name,
      level,
      maxLevel,
      village,
      archive_date,
      '#'||player_tag as player_tag
    FROM `fleet-parser-330316.luis_stage.stg_coc_player_heroes`  
  ) as s
  on t.archive_date = s.archive_date
  and t.player_tag = s.player_tag
  and t.hero_name = s.name

  WHEN MATCHED THEN UPDATE SET
    t.hero_name = s.name,
    t.hero_level = s.level,
    t.hero_max_level = s.maxlevel,
    t.village = s.village 

  WHEN NOT MATCHED THEN INSERT
    (
      hero_name, hero_level, hero_max_level, village, archive_date, player_tag
    )
    VALUES
    (
      s.name, s.level, s.maxlevel, s.village, s.archive_date, s.player_tag
    );
##Insert Hero data END  

##Insert Spell data START
  MERGE INTO`fleet-parser-330316.luistest.coc_player_spells` as t
  USING (
    SELECT
      name,
      level,
      maxLevel,
      village,
      archive_date,
      '#'||player_tag as player_tag
    FROM `fleet-parser-330316.luis_stage.stg_coc_player_spells`  
  ) as s
  on t.archive_date = s.archive_date
  and t.player_tag = s.player_tag
  and t.spell_name = s.name

  WHEN MATCHED THEN UPDATE SET
    t.spell_name = s.name,
    t.spell_level = s.level,
    t.spell_max_level = s.maxlevel,
    t.village = s.village 

  WHEN NOT MATCHED THEN INSERT
    (
      spell_name, spell_level, spell_max_level, village, archive_date, player_tag
    )
    VALUES
    (
      s.name, s.level, s.maxlevel, s.village, s.archive_date, s.player_tag
    );
##Insert Spell data END  

##Insert Troop data START
  MERGE INTO`fleet-parser-330316.luistest.coc_player_troops` as t
  USING (
    SELECT
      name,
      level,
      maxLevel,
      village,
      archive_date,
      '#'||player_tag as player_tag
    FROM `fleet-parser-330316.luis_stage.stg_coc_player_troops`  
  ) as s
  on t.archive_date = s.archive_date
  and t.player_tag = s.player_tag
  and t.troop_name = s.name
  and t.village = s.village

  WHEN MATCHED THEN UPDATE SET
    t.troop_name = s.name,
    t.troop_level = s.level,
    t.troop_max_level = s.maxlevel,
    t.village = s.village 

  WHEN NOT MATCHED THEN INSERT
    (
      troop_name, troop_level, troop_max_level, village, archive_date, player_tag
    )
    VALUES
    (
      s.name, s.level, s.maxlevel, s.village, s.archive_date, s.player_tag
    );
##Insert Troop data END  

##Insert Player Achievement data START
  MERGE INTO`fleet-parser-330316.luistest.coc_player_achievements` as t
  USING (
    SELECT
      name,
      stars,
      value,
      target,
      info,
      completionInfo,
      village,
      archive_date,
      '#'||player_tag as player_tag
    FROM `fleet-parser-330316.luis_stage.stg_coc_player_achievements`  
  ) as s
  on t.archive_date = s.archive_date
  and t.player_tag = s.player_tag
  and t.achievement_info = s.info
  and t.village = s.village

  WHEN MATCHED THEN UPDATE SET
    t.achievement_name = s.name,
    t.achievement_stars = s.stars,
    t.achievement_value = s.value,
    t.achievement_target = s.target,
    t.achievement_info = s.info,
    t.achievement_completion = s.completionInfo,
    t.village = s.village

  WHEN NOT MATCHED THEN INSERT
    (
      achievement_name, achievement_stars, achievement_value, achievement_target, achievement_info, achievement_completion, village, archive_date, player_tag
    )
    VALUES
    (
      s.name, s.stars, s.value, s.target, s.info, s.completionInfo, s.village, s.archive_date, s.player_tag
    );
##Insert Player Achievement data END  
END;